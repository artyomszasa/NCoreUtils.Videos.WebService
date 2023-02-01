using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.WebService;
using NCoreUtils.IO;

namespace NCoreUtils.Videos;

public partial class VideoAnalyzerClient : VideosClient, IVideoAnalyzer
{
    public VideoAnalyzerClient(
        VideosClientConfiguration<VideoAnalyzerClient> configuration,
        ILogger<VideosClient> logger,
        IHttpClientFactory? httpClientFactory = null)
        : base(configuration, logger, httpClientFactory)
    { }

    protected virtual async ValueTask<AnalyzeOperationContext> GetOperationContextAsync(
        IReadableResource source,
        string endpoint,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (source is ISerializableResource ssource)
        {
            // source video is json-serializable
            if (await IsJsonSerializationSupportedAsync(endpoint, cancellationToken).ConfigureAwait(false))
            {
                // both remote server and video source support json-serialization
                // NOTE: Destination is always inline --> not used on server
                var payload = new SourceAndDestination(await ssource.GetUriAsync(cancellationToken).ConfigureAwait(false), null);
                var producer = StreamProducer.Create((ouput, cancellationToken) =>
                {
                    return new ValueTask(JsonSerializer.SerializeAsync(
                        ouput,
                        payload,
                        SourceAndDestinationJsonContext.Default.SourceAndDestination,
                        cancellationToken)
                    );
                });
                return AnalyzeOperationContext.Json(producer);
            }
            // remote server does not support json-serialization
            if (Configuration.AllowInlineData)
            {
                // remote server does not support json-serialized videos but the inline data is enabled --> proceed
                Logger.LogWarning("Source video supports json serialization but remote server does not thus inline data will be used.");
                return AnalyzeOperationContext.Inline(source.CreateProducer());
            }
            // remote server does not support json-serialized vides and the inline data is disabled --> throw exception
            throw new InvalidOperationException("Source video supports json serialization but remote server does not. Either enable inline data or use compatible server.");
        }
        // source video is not json-serializable
        if (!Configuration.AllowInlineData)
        {
            throw new InvalidOperationException("Source video does not support json serialization. Either enable inline data or use json-serializable video source.");
        }
        return AnalyzeOperationContext.Inline(source.CreateProducer());
    }

    protected virtual async ValueTask<VideoInfo> InvokeGetVideoInfoAsync(
        IReadableResource source,
        string endpoint,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = Logger.BeginScope(Guid.NewGuid());
        Logger.LogDebug("Analyze operation starting.");
        var uri = new UriBuilder(endpoint).AppendPathSegment(Routes.Info).Uri;
        var context = await GetOperationContextAsync(source, endpoint, cancellationToken);
        Logger.LogDebug("Computed context for analyze operation ({ContentType}).", context.ContentType);
        try
        {
            var consumer = StreamConsumer
                .Create((input, cancellationToken) => {
                    return JsonSerializer.DeserializeAsync(input, VideoInfoJsonContext.Default.VideoInfo, cancellationToken);
                })
                .Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
                {
                    Logger.LogDebug("Sending analyze request.");
                    using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new TypedStreamContent(input, context.ContentType) };
                    using var client = CreateHttpClient();
                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    Logger.LogDebug("Received response of the analyze request.");
                    await CheckAsync(response, cancellationToken);
                    await using var stream =
#if NETSTANDARD2_1
                        await response.Content.ReadAsStreamAsync()
#else
                        await response.Content.ReadAsStreamAsync(cancellationToken)
#endif
                        .ConfigureAwait(false);
                    await stream.CopyToAsync(output, 16 * 1024, cancellationToken)
                        .ConfigureAwait(false);
                    Logger.LogDebug("Done processing response of the analyze request.");
                }));
            Logger.LogDebug("Initializing analyze operation.");
            var result = await context.Producer.ConsumeAsync(consumer, cancellationToken).ConfigureAwait(false);
            Logger.LogDebug("Analyze operation completed.");
            return result ?? new VideoInfo(default, Array.Empty<MediaStreamInfo>(), default, default);
        }
        catch (Exception exn) when (exn is not VideoException)
        {
            if (IsSocketRelated(exn, out var socketExn))
            {
                if (IsBrokenPipe(socketExn) && source.Reusable)
                {
                    Logger.LogWarning(exn, "Failed to perform operation due to connection error, retrying...");
                    return await InvokeGetVideoInfoAsync(source, endpoint, cancellationToken);
                }
                throw new RemoteVideoConnectivityException(endpoint, socketExn.SocketErrorCode, "Network related error has occured while performing operation.", exn);
            }
            throw new RemoteVideoException(endpoint, ErrorCodes.GenericError, "Error has occurred while performing operation.", exn);
        }
    }

    public virtual ValueTask<VideoInfo> AnalyzeAsync(IReadableResource source, CancellationToken cancellationToken = default)
        => InvokeGetVideoInfoAsync(source, Configuration.EndPoint, cancellationToken);
}
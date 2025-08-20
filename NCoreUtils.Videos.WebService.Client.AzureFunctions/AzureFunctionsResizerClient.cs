using System.Net.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.IO;
using NCoreUtils.Videos;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils;

public class AzureFunctionsVideoResizerClient(
    VideosClientConfiguration<VideoResizerClient> configuration,
    ILogger<VideoResizerClient> logger,
    IHttpClientFactory? httpClientFactory = null)
    : VideoResizerClient(configuration, logger, httpClientFactory)
{
    protected override async ValueTask InvokeResizeAsync(
        IReadableResource source,
        IWritableResource destination,
        string queryString,
        string endpoint,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = Logger.BeginScope(Guid.NewGuid());
        Logger.LogDebug("Resize operation starting.");
        var uri = new UriBuilder(endpoint) { Query = queryString }.AppendPathSegment("schedule").Uri;
        var context = await GetOperationContextAsync(source, destination, endpoint, cancellationToken).ConfigureAwait(false);
        if (context.Destination is not null)
        {
            throw new InvalidOperationException("Resizing using azure durabble function only supports serializable data.");
        }
        Logger.LogDebug("Computed context for resize operation ({ContentType}).", context.ContentType);
        // both source and destination are serializable
        var consumer = StreamConsumer.Create<Uri>(async (input, cancellationToken) =>
        {
            HttpContent content;
            if (Configuration.BufferRequests)
            {
                await using var bufferStream = new MemoryStream();
                await input.CopyToAsync(bufferStream, 16 * 1024, cancellationToken).ConfigureAwait(false);
                content = new ByteArrayContent(bufferStream.ToArray());
                content.Headers.ContentType = JsonStreamContent.ApplicationJson;
            }
            else
            {
                content = new JsonStreamContent(input);
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
            using var client = CreateHttpClient();
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                // FIXME: further information from response
                throw new InvalidOperationException("Failed to schedule resize.");
            }
            if (response.Headers.Location is null)
            {
                throw new InvalidOperationException("Failed to schedule resize: no location returned.");
            }
            return response.Headers.Location;
        });
        Logger.LogDebug("Scheduling resize operation.");
        var statusUri = await context.Producer.ConsumeAsync(consumer, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Resize operation scheduled.");

        bool completed = false;
        using var client = CreateHttpClient();
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationToken).ConfigureAwait(false);
            using var req = new HttpRequestMessage(HttpMethod.Get, statusUri);
            using var resp = await client
                .SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            if (resp.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                // FIXME: handle error propagation
                Logger.LogDebug("Resize operation finished ({Status}).", resp.StatusCode);
                completed = true;
            }
        }
        while (!completed);
    }
}
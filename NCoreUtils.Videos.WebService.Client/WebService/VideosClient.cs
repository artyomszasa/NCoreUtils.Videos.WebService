using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Videos.WebService
{
    public abstract class VideosClient
    {
        protected static readonly JsonSerializerOptions _sourceAndDestinationSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { SourceAndDestinationConverter.Instance }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetUriFromResponse(HttpResponseMessage response)
            => response.RequestMessage?.RequestUri?.AbsoluteUri ?? string.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsJsonCompatible(MediaTypeHeaderValue contentType)
            => contentType.MediaType == "application/json" || contentType.MediaType == "text/json" || contentType.MediaType == "text/plain";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T ThrowUnsupportedResizeMode<T>(UnsupportedResizeModeData error, string uri)
            => throw new RemoteUnsupportedResizeModeException(uri, error.ResizeMode ?? "none", error.Width, error.Height, error.Description);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T ThrowRemoteError<T>(VideoErrorData error, string uri)
            => error.ErrorCode switch
            {
                ErrorCodes.InternalError => throw new RemoteInternalVideoException(uri, ((InternalVideoErrorData)error).InternalCode, error.Description),
                ErrorCodes.InvalidVideo => throw new RemoteInvalidVideoException(uri, error.Description),
                ErrorCodes.UnsupportedVideoType => throw new RemoteUnsupportedVideoTypeException(uri, ((UnsupportedVideoTypeData)error).VideoType, error.Description),
                ErrorCodes.UnsupportedResizeMode => ThrowUnsupportedResizeMode<T>((UnsupportedResizeModeData)error, uri),
                _ => throw new RemoteVideoException(uri, error.ErrorCode, error.Description)
            };

        protected static async ValueTask CheckAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.BadRequest:
                    if (response.Content is null)
                    {
                        throw new RemoteVideoCommunicationException(
                            GetUriFromResponse(response),
                            response.StatusCode,
                            $"Remote server responded with {response.StatusCode} without message."
                        );
                    }
                    if (response.Content.Headers.ContentType is null || response.Content.Headers.ContentType.MediaType is null)
                    {
                        throw new RemoteVideoCommunicationException(
                            GetUriFromResponse(response),
                            response.StatusCode,
                            $"Remote server responded with {response.StatusCode} with message without content type."
                        );
                    }
                    if (IsJsonCompatible(response.Content.Headers.ContentType))
                    {
                        VideoErrorData? error;
                        try
                        {
                            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            error = await JsonSerializer.DeserializeAsync<VideoErrorData>(stream, ErrorSerialization.Options, cancellationToken).ConfigureAwait(false);
                            if (error is null)
                            {
                                throw new RemoteVideoCommunicationException(
                                    GetUriFromResponse(response),
                                    response.StatusCode,
                                    $"Remote server responded with {response.StatusCode} without message."
                                );
                            }
                        }
                        catch (Exception exn)
                        {
                            throw new RemoteVideoException(GetUriFromResponse(response), RemoteErrorCodes.SerializationError, "Unable to deserialize error response.", exn);
                        }
                        ThrowRemoteError<int>(error, GetUriFromResponse(response));
                        return;
                    }
                    throw new RemoteVideoCommunicationException(
                        GetUriFromResponse(response),
                        response.StatusCode,
                        $"Remote server responded with {response.StatusCode} with message with unsupported content type ({response.Content.Headers.ContentType})."
                    );
                default:
                    throw new RemoteVideoCommunicationException(
                        GetUriFromResponse(response),
                        response.StatusCode,
                        $"Remote server responded with {response.StatusCode}."
                    );
            }
        }

        protected static bool IsBrokenPipe(SocketException exn)
            => -1 != exn.Message.IndexOf("broken pipe", StringComparison.OrdinalIgnoreCase);

        protected static bool IsSocketRelated(Exception exn, out SocketException socketExn)
        {
            return IsSocketRelatedLoop(new HashSet<Exception>(ReferenceEqualityComparer<Exception>.Instance), exn, out socketExn);

            static bool IsSocketRelatedLoop(HashSet<Exception> loop, Exception exn, out SocketException socketExn)
            {
                if (!loop.Add(exn))
                {
                    socketExn = default!;
                    return false;
                }
                switch (exn)
                {
                    case SocketException sexn:
                        socketExn = sexn;
                        return true;
                    case AggregateException aexn:
                        foreach (var e in aexn.InnerExceptions)
                        {
                            if (IsSocketRelatedLoop(loop, e, out socketExn))
                            {
                                return true;
                            }
                        }
                        break;
                    default:
                        if (null != exn.InnerException && IsSocketRelatedLoop(loop, exn.InnerException, out socketExn))
                        {
                            return true;
                        }
                        break;
                }
                socketExn = default!;
                return false;
            }
        }

        readonly IHttpClientFactory? _httpClientFactory;

        HashSet<string>? _cachedCapabilities;

        protected VideosClientConfiguration Configuration { get; }

        protected ILogger Logger { get; }

        protected VideosClient(
            VideosClientConfiguration configuration,
            ILogger<VideosClient> logger,
            IHttpClientFactory? httpClientFactory = default)
        {
            Configuration = configuration;
            Logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        async Task<HashSet<string>> GetCapabilitiesAsync(string endpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var uri = new UriBuilder(endpoint).AppendPathSegment(Routes.Capabilities).Uri;
            try
            {
                using var client = CreateHttpClient();
                using var stream = await client.GetStreamAsync(uri).ConfigureAwait(false);
                var capabilities = new HashSet<string>(await JsonSerializer.DeserializeAsync<string[]>(stream).ConfigureAwait(false) ?? Array.Empty<string>());
                Logger.LogDebug("Remote server supports following extensions: {0}.", string.Join(", ", capabilities));
                _cachedCapabilities = capabilities;
                return capabilities;
            }
            catch (Exception exn)
            {
                Logger.LogWarning(exn, "Querying extensions from {0} failed ({1}), assuming no extensions supported.", endpoint, exn.Message);
                return new HashSet<string>();
            }
        }

        protected virtual ValueTask<bool> IsJsonSerializationSupportedAsync(string endpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Configuration.CacheCapabilities && null != _cachedCapabilities)
            {
                return new ValueTask<bool>(_cachedCapabilities.Contains(Capabilities.JsonSerializedVideoInfo));
            }
            return ContinueIsJsonSerializationSupportedAsync();

            async ValueTask<bool> ContinueIsJsonSerializationSupportedAsync()
            {
                var capabilities = await GetCapabilitiesAsync(endpoint, cancellationToken).ConfigureAwait(false);
                return capabilities.Contains(Capabilities.JsonSerializedVideoInfo);
            }
        }

        protected virtual HttpClient CreateHttpClient()
            => _httpClientFactory?.CreateClient(Configuration.HttpClient) ?? new HttpClient();
    }
}
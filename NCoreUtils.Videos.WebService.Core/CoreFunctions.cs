using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos
{
    public class CoreFunctions
    {
        private static byte[] Capabilities { get; } = JsonSerializer.SerializeToUtf8Bytes(
            new [] { WebService.Capabilities.JsonSerializedVideoInfo },
            StringArrayJsonContext.Default.StringArray
        );

        private static HashSet<string> Truthy { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "t",
            "on",
            "1"
        };

        internal static T NotSupportedUri<T>(Uri? uri)
            => throw new VideoResizerException("unsupported_uri", $"Either invalid or unsupported uri: {uri}.");

        private static bool IsJsonCompatible(string? contentType)
            => contentType is not null &&
                (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                    || contentType.StartsWith("text/json", StringComparison.OrdinalIgnoreCase)
                    || contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase));

#pragma warning disable IDE0060
        private static ResizeOptions ReadResizeOptions(IResourceFactory resourceFactory, IQueryCollection query)
        {
            return new ResizeOptions(
                audioType: S("a"),
                videoType: S("t"),
                width: I("w"),
                height: I("h"),
                resizeMode: S("m"),
                // watermark: S("wm"),
                quality: I("q"),
                optimize: B("x"),
                weightX: I("cx"),
                weightY: I("cy")
                //filters: FilterParser.Parse(resourceFactory, S("f"))
            );
#pragma warning restore IDE0060

            bool? B(string name)
            {
                return S(name) switch
                {
                    null => default,
                    string s => Truthy.Contains(s)
                };
            }

            int? I(string name)
            {
                return S(name) switch
                {
                    null => default,
                    string s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? (int?)i : default
                };
            }

            string? S(string name)
            {
                return query.TryGetValue(name, out var values) && values.Count > 0 ? values[0] : default;
            }
        }



        private static ValueTask<SourceAndDestination> ParseSourceAndDestination(
            HttpRequest request,
            CancellationToken cancellationToken)
        {
            if (IsJsonCompatible(request.ContentType))
            {
                return JsonSerializer.DeserializeAsync<SourceAndDestination>(
                    request.Body,
                    SourceAndDestinationJsonContext.Default.SourceAndDestination,
                    cancellationToken
                );
            }
            return default;
        }

        private static (IVideoSource Source, IVideoDestination Destination) ResolveSourceAndDestination(
            IResourceFactory resourceFactory,
            SourceAndDestination sd)
        {
            var source = resourceFactory.CreateSource(sd.Source, () => NotSupportedUri<IVideoSource>(sd.Source));
            var destination = resourceFactory.CreateDestination(sd.Destination, () => NotSupportedUri<IVideoDestination>(sd.Destination));
            return (source, destination);
        }

        public static async Task InvokeCapabilities(HttpResponse response, CancellationToken cancellationToken)
        {
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength = Capabilities.Length;
            await response.BodyWriter.WriteAsync(Capabilities, cancellationToken).ConfigureAwait(false);
            await response.BodyWriter.CompleteAsync().ConfigureAwait(false);
        }

        public static async Task InvokeResize(
            HttpRequest request,
            IResourceFactory resourceFactory,
            IVideoResizer resizer,
            CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var (source, destination) = ResolveSourceAndDestination(resourceFactory, sourceAndDestination);
            await resizer.ResizeAsync(source, destination, ReadResizeOptions(resourceFactory, request.Query), cancellationToken).ConfigureAwait(false);
        }

        public static async Task InvokeAnalyze(
            HttpRequest request,
            HttpResponse response,
            IResourceFactory resourceFactory,
            IVideoAnalyzer analyzer,
            CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var source = resourceFactory.CreateSource(sourceAndDestination.Source, () => NotSupportedUri<IVideoSource>(sourceAndDestination.Source));
            var info = await analyzer.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
            response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(response.Body, info, cancellationToken: cancellationToken).ConfigureAwait(false);
            await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task InvokeThumbnail(
            HttpRequest request,
            IResourceFactory resourceFactory,
            IVideoResizer resizer,
            CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var (source, destination) = ResolveSourceAndDestination(resourceFactory, sourceAndDestination);
            await resizer.CreateThumbnailAsync(source, destination, new ResizeOptions(), cancellationToken);
        }
    }
}
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.IO;
using NCoreUtils.Videos.Internal;
using NCoreUtils.Videos.Logging;

namespace NCoreUtils.Videos
{
    public class VideoResizer : IVideoResizer, IVideoAnalyzer
    {
        protected ILogger Logger { get; }

        protected IVideoProvider Provider { get; }

        protected IVideoResizerOptions Options { get; }

        protected ResizerCollection Resizers { get; }

        public VideoResizer(ILogger<VideoResizer> logger, IVideoProvider provider, IVideoResizerOptions options, ResizerCollection resizers)
        {
            Logger = logger;
            Provider = provider;
            Options = options;
            Resizers = resizers;
        }

        private IStreamTransformation CreateTransformation(Action<string> setContentType, ResizeOptions options)
        {
            // Logger.LogDebug("Creating transformation with options {0}", options);
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.Log(LogLevel.Debug, default, new CreatingTransformationEntry(options), default!, CreatingTransformationEntry.Formatter);
            }
            var resizeMode = options.ResizeMode ?? ResizeModes.None;
            if (Resizers.TryGetValue(resizeMode, out var resizerFactory))
            {
                return StreamTransformation.Create((input, output, cancellationToken) => TransformAsync(input, output, resizerFactory, options, setContentType, cancellationToken));
            }
            throw new UnsupportedResizeModeException(resizeMode, options.Width, options.Height, "Specified resize mode is not supported.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DecideQuality(ResizeOptions options, string imageType)
            => options.Quality ?? Options.Quality(imageType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DecideOptimize(ResizeOptions options, string imageType)
            => options.Optimize ?? Options.Optimize(imageType);

        protected virtual async ValueTask TransformAsync(
            Stream input,
            Stream output,
            IResizerFactory resizerFactory,
            ResizeOptions options,
            Action<string> setContentType,
            CancellationToken cancellationToken)
        {
            await using var video = await Provider.FromStreamAsync(input, cancellationToken);
            // await image.NormalizeAsync(cancellationToken);
            // var (isExplicit, imageType) = DecideImageType(options, image);
            var (isExplicit, videoType) = (false, "mp4");
            var quality = DecideQuality(options, videoType);
            var optimize = DecideOptimize(options, videoType);
            // Logger.LogDebug (
            //   "Resizing image with computed options [ImageType = {0}, Quality = {1}, Optimization = {2}]",
            //   isExplicit ? imageType : $"{imageType} (implicit)",
            //   quality,
            //   optimize
            // );
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.Log(LogLevel.Debug, default, new ResizingVideoEntry(videoType, isExplicit, quality, optimize), default!, ResizingVideoEntry.Formatter);
            }
            setContentType(/* ImageTypes.ToMediaType(imageType) */ "video/mp4");
            var resizer = resizerFactory.CreateResizer(video, options);
            await resizer.ResizeAsync(video, cancellationToken);
            // foreach (var filter in options.Filters)
            // {
            //     await image.ApplyFilterAsync(filter, cancellationToken);
            // }
            await video.WriteToAsync(output, videoType, quality, optimize, cancellationToken);
            await output.FlushAsync(cancellationToken);
            output.Close();
        }

        public ValueTask ResizeAsync(
            IVideoSource source,
            IVideoDestination destination,
            ResizeOptions options,
            CancellationToken cancellationToken = default)
        {
            string? contentType = null;
            return source.CreateProducer()
                .Chain(CreateTransformation(ct => contentType = ct, options))
                .ConsumeAsync(StreamConsumer.Delay(_ =>
                {
                    var ct = contentType ?? "application/octet-stream";
                    // Logger.LogDebug("Initializing image destination with content type {0}.", ct);
                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.Log(LogLevel.Debug, default, new InitializingDestinationEntry(ct), default!, InitializingDestinationEntry.Formatter);
                    }
                    return new ValueTask<IStreamConsumer>(destination.CreateConsumer(new ContentInfo(ct)));
                }), cancellationToken);
        }

        public ValueTask CreateThumbnailAsync(
            IVideoSource source,
            IVideoDestination destination,
            ResizeOptions options,
            CancellationToken cancellationToken = default)
            => source.CreateProducer()
                .Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
                {
                    await using var video = await Provider.FromStreamAsync(input, cancellationToken);
                    await video.WriteThumbnailAsync(output, TimeSpan.FromSeconds(10), cancellationToken);
                }))
                .ConsumeAsync(destination.CreateConsumer(new ContentInfo("image/png")), cancellationToken);

        public ValueTask<VideoInfo> AnalyzeAsync(IVideoSource source, CancellationToken cancellationToken)
            => source.CreateProducer()
                .ConsumeAsync(StreamConsumer.Create(async (input, cancellationToken) =>
                {
                    await using var video = await Provider.FromStreamAsync(input, cancellationToken);
                    return await video.GetVideoInfoAsync(cancellationToken);
                }), cancellationToken);
    }
}
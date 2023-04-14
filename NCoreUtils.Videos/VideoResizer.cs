using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.IO;
using NCoreUtils.Videos.Internal;
using NCoreUtils.Videos.Logging;

namespace NCoreUtils.Videos;

public class VideoResizer : IVideoResizer, IVideoAnalyzer
{
    public const int DefaultBufferSize = 32 * 1024;

    private static Random Random { get; } = new(unchecked((int)(DateTimeOffset.Now.UtcTicks % ((long)int.MaxValue + 1L))));

    private static string GetTempFileName()
        => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{Random.Next(100):D2}");

    protected ILogger Logger { get; }

    protected IVideoProvider Provider { get; }

    protected IVideoResizerOptions Options { get; }

    protected ResizerCollection Resizers { get; }

    public VideoResizer(ILogger<VideoResizer> logger, IVideoProvider provider, ResizerCollection resizers, IVideoResizerOptions? options = default)
    {
        Logger = logger;
        Provider = provider;
        Options = options ?? VideoResizerOptions.Default;
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
    private int DecideQuality(ResizeOptions options, VideoSettings? videoSettings)
        => options.Quality ?? Options.Quality(videoSettings?.Codec ?? "mp4");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DecideOptimize(ResizeOptions options, VideoSettings? videoSettings)
        => options.Optimize ?? Options.Optimize(videoSettings?.Codec ?? "mp4");

    protected virtual async ValueTask TransformAsync(
        Stream input,
        Stream output,
        IResizerFactory resizerFactory,
        ResizeOptions options,
        Action<string> setContentType,
        CancellationToken cancellationToken)
    {
        // FIXME: generalize buffering
        // FIXME: MOOV?
        await using var bufferedOutput = new FileStream(
            path: GetTempFileName(),
            mode: FileMode.CreateNew,
            access: FileAccess.ReadWrite,
            share: FileShare.None,
            bufferSize: DefaultBufferSize,
            options: FileOptions.Asynchronous | FileOptions.DeleteOnClose
        );
        // NOTE: mp4 may contain moov data at the end of the stream --> stream must be buffered before
        // processing
        // FIXME: generalize buffering
        await using var bufferedInput = new FileStream(
            path: GetTempFileName(),
            mode: FileMode.CreateNew,
            access: FileAccess.ReadWrite,
            share: FileShare.None,
            bufferSize: DefaultBufferSize,
            options: FileOptions.RandomAccess | FileOptions.DeleteOnClose | FileOptions.Asynchronous
        );
        await input.CopyToAsync(bufferedInput, DefaultBufferSize, cancellationToken).ConfigureAwait(false);
        bufferedInput.Seek(0, SeekOrigin.Begin);
        await using var video = await Provider.FromStreamAsync(bufferedInput, cancellationToken).ConfigureAwait(false);
        var (isExplicit, videoSettings) = (false, options.VideoType);
        var quality = DecideQuality(options, videoSettings);
        var optimize = DecideOptimize(options, videoSettings);
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.Log(LogLevel.Debug, default, new ResizingVideoEntry(videoSettings?.Codec ?? "mp4", isExplicit, quality, optimize), default!, ResizingVideoEntry.Formatter);
        }
        setContentType(/* TODO: dynamic */ "video/mp4");
        var resizer = resizerFactory.CreateResizer(video, options);
        var videoTransformations = await resizer.PopulateTransformations(video)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        await video.WriteToAsync(
            bufferedOutput,
            videoTransformations,
            videoSettings,
            options.AudioType ?? video.AudioCodec,
            quality,
            optimize,
            cancellationToken
        ).ConfigureAwait(false);
        await bufferedOutput.FlushAsync(cancellationToken);
        bufferedOutput.Seek(0L, SeekOrigin.Begin);
        await bufferedOutput.CopyToAsync(output, 32 * 1024, cancellationToken).ConfigureAwait(false);
        // await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        output.Close();
    }

    public ValueTask CreateThumbnailAsync(
        IReadableResource source,
        IWritableResource destination,
        ResizeOptions options,
        CancellationToken cancellationToken = default)
        => source.CreateProducer()
            .Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
            {
                // NOTE: mp4 may contain moov data at the end of the stream --> stream must be buffered before
                // processing
                // FIXME: generalize buffering
                await using var bufferedInput = new FileStream(
                    path: GetTempFileName(),
                    mode: FileMode.CreateNew,
                    access: FileAccess.ReadWrite,
                    share: FileShare.None,
                    bufferSize: DefaultBufferSize,
                    options: FileOptions.RandomAccess | FileOptions.DeleteOnClose | FileOptions.Asynchronous
                );
                await input.CopyToAsync(bufferedInput, DefaultBufferSize, cancellationToken).ConfigureAwait(false);
                bufferedInput.Seek(0, SeekOrigin.Begin);
                await using var video = await Provider.FromStreamAsync(bufferedInput, cancellationToken).ConfigureAwait(false);
                await video.WriteThumbnailAsync(output, TimeSpan.FromSeconds(10), cancellationToken);
            }))
            .ConsumeAsync(destination.CreateConsumer(new ResourceInfo("image/jpeg")), cancellationToken);

    public ValueTask<VideoInfo> AnalyzeAsync(IReadableResource source, CancellationToken cancellationToken)
        => source.CreateProducer()
            .ConsumeAsync(StreamConsumer.Create(async (input, cancellationToken) =>
            {
                await using var video = await Provider.FromStreamAsync(input, cancellationToken);
                return await video.GetVideoInfoAsync(cancellationToken);
            }), cancellationToken);

    public ValueTask ResizeAsync(
        IReadableResource source,
        IWritableResource destination,
        ResizeOptions options,
        CancellationToken cancellationToken = default)
    {
        string? contentType = null;
        return source.CreateProducer()
            .Chain(CreateTransformation(ct => contentType = ct, options))
            .ConsumeAsync(StreamConsumer.Delay(_ =>
            {
                var ct = contentType ?? "application/octet-stream";
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.Log(LogLevel.Debug, default, new InitializingDestinationEntry(ct), default!, InitializingDestinationEntry.Formatter);
                }
                return new ValueTask<IStreamConsumer>(destination.CreateConsumer(new ResourceInfo(ct)));
            }), cancellationToken);
    }
}
using Microsoft.Extensions.Logging;
using NCoreUtils.FFMpeg;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos.FFMpeg;

public class VideoProvider(
    ILoggerFactory? loggerFactory = default,
    IVideoProcessorConfiguration? configuration = default)
    : IVideoProvider
{
    private sealed class DummyScope : IDisposable
    {
        private static DummyScope? _singleton;

        public static DummyScope Singleton { get; } = _singleton ??= new();

        private DummyScope() { }

        public void Dispose() { /* noop */ }
    }

    private sealed class NoopLogger : ILogger<Video>
    {
        private static NoopLogger? _singleton;

        public static NoopLogger Singleton => _singleton ??= new();

        private NoopLogger() { }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => DummyScope.Singleton;

        public bool IsEnabled(LogLevel logLevel)
            => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { /* noop */ }
    }

    private ILoggerFactory? LoggerFactory { get; } = loggerFactory;

    public IVideoProcessorConfiguration Configuration { get; } = configuration ?? VideoProcessorConfiguration.Default;

    private ILogger<Video> CreateLogger()
        => LoggerFactory is ILoggerFactory loggerFactory
            ? loggerFactory.CreateLogger<Video>()
            : NoopLogger.Singleton;

    public Video FromStream(Stream source)
    {
        var inCtx = AVFormatInputContext.CreateInputContext(source);
        inCtx.FindStreamInfo();
        int? videoStreamIndex = default;
        int? audioStreamIndex = default;
        foreach (var stream in inCtx.Streams)
        {
            if (stream.CodecParameters.CodecType == AVMediaType.AVMEDIA_TYPE_VIDEO && videoStreamIndex is null)
            {
                videoStreamIndex = stream.Index;
            }
            if (stream.CodecParameters.CodecType == AVMediaType.AVMEDIA_TYPE_AUDIO && audioStreamIndex is null)
            {
                audioStreamIndex = stream.Index;
            }
        }
        return new(CreateLogger(), inCtx, videoStreamIndex, audioStreamIndex, Configuration.SingleThread);
    }

    public async ValueTask<IVideo> FromStreamAsync(Stream source, CancellationToken cancellationToken = default)
    {
        await Task.Yield(); // FORCE ASYNC
        return FromStream(source);
    }
}
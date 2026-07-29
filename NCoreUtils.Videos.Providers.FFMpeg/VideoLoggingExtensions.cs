using Microsoft.Extensions.Logging;
using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal static partial class VideoLoggingExtensions
{
    public const int AudioCodecContextInitialized = 10000;

    public const int PipelineDoneReadingNoMoreFrames = 10100;

    public const int PipelineReadingCancelled = 10101;

    public const int PipelineReadingFailed = 10102;

    public const int PipelineWritingFailed = 10104;

    [LoggerMessage(
        EventId = AudioCodecContextInitialized,
        EventName = nameof(AudioCodecContextInitialized),
        Level = LogLevel.Information,
        Message = "Initialized audio codec context [{role}]: {codec}, {timeBase}, {sampleFormat} / {sampleRate}, {fieldOrder}.",
        SkipEnabledCheck = false)]
    public static partial void LogAudioCodecContextInitialized(
        this ILogger logger,
        string role,
        string? codec,
        AVRational timeBase,
        AVSampleFormat sampleFormat,
        int sampleRate,
        AVFieldOrder fieldOrder
    );

    public static void LogAudioCodecContextInitialized(this ILogger logger, string role, AVCodecContext codecContext)
        => logger.LogAudioCodecContextInitialized(
            role,
            codecContext.Codec.Name,
            codecContext.TimeBase,
            codecContext.SampleFormat,
            codecContext.SampleRate,
            codecContext.FieldOrder
        );

    [LoggerMessage(
        EventId = PipelineDoneReadingNoMoreFrames,
        EventName = nameof(PipelineDoneReadingNoMoreFrames),
        Level = LogLevel.Information,
        Message = "Pipeline/reading completed: no more frames.",
        SkipEnabledCheck = false)]
    public static partial void LogPipelineDoneReadingNoMoreFrames(this ILogger logger);

    [LoggerMessage(
        EventId = PipelineReadingCancelled,
        EventName = nameof(PipelineReadingCancelled),
        Level = LogLevel.Information,
        Message = "Pipeline/reading cancelled.",
        SkipEnabledCheck = false)]
    public static partial void LogPipelineReadingCancelled(this ILogger logger);

    [LoggerMessage(
        EventId = PipelineReadingFailed,
        EventName = nameof(PipelineReadingFailed),
        Level = LogLevel.Error,
        Message = "Pipeline/reading failed.",
        SkipEnabledCheck = false)]
    public static partial void LogPipelineReadingFailed(this ILogger logger, Exception exn);

    [LoggerMessage(
        EventId = PipelineWritingFailed,
        EventName = nameof(PipelineWritingFailed),
        Level = LogLevel.Error,
        Message = "Pipeline/writing failed.",
        SkipEnabledCheck = false)]
    public static partial void LogPipelineWritingFailed(this ILogger logger, Exception exn);
}
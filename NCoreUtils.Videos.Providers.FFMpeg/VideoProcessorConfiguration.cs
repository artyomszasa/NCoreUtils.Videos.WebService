namespace NCoreUtils.Videos.FFMpeg;

public sealed class VideoProcessorConfiguration(bool singleThread) : IVideoProcessorConfiguration
{
    public const bool DefaultSingleThread = false;

    public static VideoProcessorConfiguration Default { get; } = new(singleThread: DefaultSingleThread);

    public bool SingleThread { get; } = singleThread;
}
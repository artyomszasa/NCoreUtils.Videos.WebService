using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal static class Helpers
{
    public static Dictionary<int, AVRational> Add(this Dictionary<int, AVRational> source, Video.TransformationInfo? tx0)
    {
        if (tx0 is Video.TransformationInfo tx)
        {
            source.Add(tx.OutStreamIndex, tx.PreOutTimeBase);
        }
        return source;
    }

    public static Dictionary<int, IConsumer<AVPacket>> Add(this Dictionary<int, IConsumer<AVPacket>> source, int? outStreamIndex, IConsumer<AVPacket>? consumer)
    {
        if (outStreamIndex is int index && consumer is not null)
        {
            source.Add(index, consumer);
        }
        return source;
    }
}
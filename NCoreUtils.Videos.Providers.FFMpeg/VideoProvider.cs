using NCoreUtils.FFMpeg;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos.FFMpeg;

public class VideoProvider : IVideoProvider
{
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
        return new(inCtx, videoStreamIndex, audioStreamIndex);
    }

    public async ValueTask<IVideo> FromStreamAsync(Stream source, CancellationToken cancellationToken = default)
    {
        await Task.Yield(); // FORCE ASYNC
        return FromStream(source);
    }
}
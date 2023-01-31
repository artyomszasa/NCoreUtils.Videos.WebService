using System;
using System.Collections.Generic;
using System.Linq;

namespace NCoreUtils;

public class VideoInfo
{
    public TimeSpan Duration { get; }

    public IReadOnlyList<MediaStreamInfo> Streams { get; }

    public int? VideoStreamIndex { get; }

    public int? AudioStreamIndex { get; }

    public MediaStreamInfo? VideoStream
        => VideoStreamIndex is int index
            ? Streams.FirstOrDefault(stream => stream.Index == index)
            : default;

    public MediaStreamInfo? AudioStream
        => AudioStreamIndex is int index
            ? Streams.FirstOrDefault(stream => stream.Index == index)
            : default;

    public VideoInfo(
        TimeSpan duration,
        IReadOnlyList<MediaStreamInfo> streams,
        int? videoStreamIndex,
        int? audioStreamIndex)
    {
        Duration = duration;
        Streams = streams ?? throw new ArgumentNullException(nameof(streams));
        VideoStreamIndex = videoStreamIndex;
        AudioStreamIndex = audioStreamIndex;
    }
}
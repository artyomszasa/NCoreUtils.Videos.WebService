using System.Collections.Generic;

namespace NCoreUtils.Videos;

public class VideoResizerOptions : IVideoResizerOptions
{
    public static IVideoResizerOptions Default { get; } = new VideoResizerOptions();

    public long? MemoryLimit { get; set; }

    public Dictionary<string, int> Quality { get; } = new();

    public Dictionary<string, bool> Optimize { get; } = new();

    int IVideoResizerOptions.Quality(string videoType)
        => Quality.TryGetValue(videoType, out var i) ? i : 85;

    bool IVideoResizerOptions.Optimize(string videoType)
        => Optimize.TryGetValue(videoType, out var b) && b;
}
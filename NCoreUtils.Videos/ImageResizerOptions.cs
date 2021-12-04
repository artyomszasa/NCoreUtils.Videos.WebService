using System.Collections.Generic;

namespace NCoreUtils.Videos
{
    public class VideoResizerOptions : IVideoResizerOptions
    {
        public static IVideoResizerOptions Default { get; } = new VideoResizerOptions();

        public long? MemoryLimit { get; set; }

        public Dictionary<string, int> Quality { get; } = new();

        public Dictionary<string, bool> Optimize { get; } = new();

        int IVideoResizerOptions.Quality(string imageType)
            => Quality.TryGetValue(imageType, out var i) ? i : 85;

        bool IVideoResizerOptions.Optimize(string imageType)
            => Optimize.TryGetValue(imageType, out var b) && b;
    }
}
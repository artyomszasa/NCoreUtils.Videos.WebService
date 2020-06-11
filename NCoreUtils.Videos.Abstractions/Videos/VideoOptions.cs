namespace NCoreUtils.Videos
{
    public class VideoOptions
    {
        public string? VideoType { get; }

        public int? Width { get; }

        public int? Height { get; }

        public int? Quality { get; }

        public VideoOptions(string? videoType, int? width, int? height, int? quality)
        {
            VideoType = videoType;
            Width = width;
            Height = height;
            Quality = quality;
        }
    }
}
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos.WebService
{
    public class VideosClientConfiguration
    {
        public const string DefaultHttpClient = "NCoreUtils.Videos";

        public string EndPoint { get; set; } = string.Empty;

        public string HttpClient { get; set; } = DefaultHttpClient;

        public bool AllowInlineData { get; set; } = false;

        public bool CacheCapabilities { get; set; } = true;
    }
}
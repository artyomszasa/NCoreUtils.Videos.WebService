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

        public bool BufferRequests { get; set; } = false;

        internal VideosClientConfiguration<T> AsTyped<T>()
            where T : VideosClient
            => new()
            {
                EndPoint = EndPoint,
                AllowInlineData = AllowInlineData,
                CacheCapabilities = CacheCapabilities,
                BufferRequests = BufferRequests,
                HttpClient = HttpClient
            };
    }

    public class VideosClientConfiguration<T> : VideosClientConfiguration
        where T : VideosClient
    { }
}
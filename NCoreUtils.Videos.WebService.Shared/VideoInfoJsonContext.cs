using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService
{
    [JsonSerializable(typeof(VideoInfo))]
    public partial class VideoInfoJsonContext : JsonSerializerContext
    {

    }
}
using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService;

[JsonSerializable(typeof(VideoErrorData))]
internal partial class VideoErrorSerializerContext : JsonSerializerContext { }
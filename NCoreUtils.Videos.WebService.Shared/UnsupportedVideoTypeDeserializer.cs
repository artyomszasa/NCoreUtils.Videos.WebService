using System.Text;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    sealed class UnsupportedVideoTypeDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        static readonly byte[] _keyVideoType = Encoding.ASCII.GetBytes(VideoErrorProperties.VideoType);

        public string VideoType { get; set; } = string.Empty;

        public UnsupportedVideoTypeDeserializer(string errorCode) : base(errorCode) { }

        public VideoErrorData CreateInstance()
            => new UnsupportedVideoTypeData(ErrorCode, Description, VideoType);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString() ?? string.Empty;
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyVideoType))
            {
                reader.ReadOrFail();
                VideoType = reader.GetString() ?? string.Empty;
                reader.ReadOrFail();
            }
            else
            {
                reader.ReadOrFail();
                reader.Skip();
            }
        }
    }
}
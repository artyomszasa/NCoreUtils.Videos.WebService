using System.Text;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    sealed class UnsupportedResizeModeDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        static readonly byte[] _keyResizeMode = Encoding.ASCII.GetBytes(VideoErrorProperties.ResizeMode);

        static readonly byte[] _keyWidth = Encoding.ASCII.GetBytes(VideoErrorProperties.Width);

        static readonly byte[] _keyHeight = Encoding.ASCII.GetBytes(VideoErrorProperties.Height);

        string? _resizeMode;

        int? _width;

        int? _height;

        public UnsupportedResizeModeDeserializer(string errorCode) : base(errorCode) { }

        public VideoErrorData CreateInstance()
            => new UnsupportedResizeModeData(ErrorCode, Description, _resizeMode, _width, _height);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString() ?? string.Empty;
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyResizeMode))
            {
                reader.ReadOrFail();
                _resizeMode = reader.GetString();
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyWidth))
            {
                reader.ReadOrFail();
                _width = reader.GetInt32();
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyHeight))
            {
                reader.ReadOrFail();
                _height = reader.GetInt32();
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
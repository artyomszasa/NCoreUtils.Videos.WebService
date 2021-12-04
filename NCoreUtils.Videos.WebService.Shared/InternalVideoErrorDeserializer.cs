using System.Text;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    sealed class InternalVideoErrorDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        static readonly byte[] _keyInternalCode = Encoding.ASCII.GetBytes(VideoErrorProperties.InternalCode);

        string _internalCode = string.Empty;

        public InternalVideoErrorDeserializer(string errorCode) : base(errorCode) { }

        public VideoErrorData CreateInstance()
            => new InternalVideoErrorData(ErrorCode, Description, _internalCode);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString() ?? string.Empty;
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyInternalCode))
            {
                reader.ReadOrFail();
                _internalCode = reader.GetString() ?? string.Empty;
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
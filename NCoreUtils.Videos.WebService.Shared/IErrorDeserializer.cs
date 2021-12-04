using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    public interface IErrorDeserializer
    {
        VideoErrorData CreateInstance();

        void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options);
    }
}
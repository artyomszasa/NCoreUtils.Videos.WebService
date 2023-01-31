using System.Text.Json;

namespace NCoreUtils.Videos.WebService;

internal class GenericErrorDeserializer : ErrorDeserializer, IErrorDeserializer
{
    public GenericErrorDeserializer(string errorCode) : base(errorCode) { }

    public VideoErrorData CreateInstance()
        => new(ErrorCode, Description);

    public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.ValueTextEquals(_keyDescription))
        {
            reader.ReadOrFail();
            Description = reader.GetString() ?? string.Empty;
            reader.ReadOrFail();
        }
        else
        {
            reader.ReadOrFail();
            reader.Skip();
        }
    }
}
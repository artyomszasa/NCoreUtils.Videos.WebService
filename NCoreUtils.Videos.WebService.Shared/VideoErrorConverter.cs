using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService;

internal sealed class VideoErrorConverter : JsonConverter<VideoErrorData>
{
    static readonly byte[] _keyErrorCode = Encoding.ASCII.GetBytes(VideoErrorProperties.ErrorCode);

    static readonly Dictionary<string, Func<string, IErrorDeserializer>> _deserializers = new()
    {
        { ErrorCodes.InternalError, errorCode => new InternalVideoErrorDeserializer(errorCode) },
        { ErrorCodes.UnsupportedVideoType, errorCode => new UnsupportedVideoTypeDeserializer(errorCode) },
        { ErrorCodes.UnsupportedResizeMode, errorCode => new UnsupportedResizeModeDeserializer(errorCode) }
    };

    public override VideoErrorData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.None:
            case JsonTokenType.Null:
                return default!;
            case JsonTokenType.StartObject:
                reader.ReadOrFail();
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(_keyErrorCode))
                {
                    reader.ReadOrFail();
                    var errorCode = reader.GetString() ?? string.Empty;
                    var deserializer = _deserializers.TryGetValue(errorCode, out var factory)
                        ? factory(errorCode)
                        : new GenericErrorDeserializer(errorCode);
                    reader.ReadOrFail();
                    while (reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            deserializer.ReadProperty(ref reader, options);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid json token encountered.");
                        }
                    }
                    return deserializer.CreateInstance();
                }
                throw new InvalidOperationException("First property must be an error_code.");
            default:
                throw new InvalidOperationException("Invalid json token encountered.");
        }
    }

    public override void Write(Utf8JsonWriter writer, VideoErrorData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        value.WriteTo(writer);
        writer.WriteEndObject();
    }
}
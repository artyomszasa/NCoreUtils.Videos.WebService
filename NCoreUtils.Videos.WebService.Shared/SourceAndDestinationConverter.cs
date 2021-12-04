using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService
{
    public class SourceAndDestinationConverter : JsonConverter<SourceAndDestination>
    {
        static readonly byte[] _keySource = Encoding.ASCII.GetBytes(SourceAndDestinationProperties.Source);

        static readonly byte[] _keyDestination = Encoding.ASCII.GetBytes(SourceAndDestinationProperties.Destination);

        static readonly JsonEncodedText _jsonSource = JsonEncodedText.Encode(SourceAndDestinationProperties.Source);

        static readonly JsonEncodedText _jsonDestination = JsonEncodedText.Encode(SourceAndDestinationProperties.Destination);

        public static SourceAndDestinationConverter Instance { get; } = new SourceAndDestinationConverter();

        public override SourceAndDestination Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.ReadOrFail();
                Uri? source = default;
                Uri? destination = default;
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        if (reader.ValueTextEquals(_keySource))
                        {
                            reader.ReadOrFail();
                            source = reader.TokenType switch
                            {
                                JsonTokenType.Null => default,
                                JsonTokenType.String => new Uri(reader.GetString() ?? string.Empty, UriKind.Absolute),
                                JsonTokenType token => throw new InvalidOperationException($"Expected source Uri or null, got {token}.")
                            };
                            reader.ReadOrFail();
                        }
                        else if (reader.ValueTextEquals(_keyDestination))
                        {
                            reader.ReadOrFail();
                            destination = reader.TokenType switch
                            {
                                JsonTokenType.Null => default,
                                JsonTokenType.String => new Uri(reader.GetString() ?? string.Empty, UriKind.Absolute),
                                JsonTokenType token => throw new InvalidOperationException($"Expected destination Uri or null, got {token}.")
                            };
                            reader.ReadOrFail();
                        }
                        else
                        {
                            reader.ReadOrFail();
                            reader.Skip();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Expected property name, got {reader.TokenType}.");
                    }
                }
                reader.Read();
                return new SourceAndDestination(source, destination);
            }
            throw new InvalidOperationException($"Expected object start, got {reader.TokenType}.");
        }

        public override void Write(Utf8JsonWriter writer, SourceAndDestination value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(_jsonSource, value.Source?.AbsoluteUri);
            writer.WriteString(_jsonDestination, value.Destination?.AbsoluteUri);
            writer.WriteEndObject();
        }
    }
}
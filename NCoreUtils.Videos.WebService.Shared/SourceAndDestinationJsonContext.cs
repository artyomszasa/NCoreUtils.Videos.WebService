using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService;

[JsonSerializable(typeof(SourceAndDestination))]
public partial class SourceAndDestinationJsonContext : JsonSerializerContext { }
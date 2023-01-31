using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService;

[JsonSerializable(typeof(string[]))]
public partial class StringArrayJsonContext : JsonSerializerContext { }
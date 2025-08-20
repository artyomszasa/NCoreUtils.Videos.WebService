using System.IO;
using System.Net.Http.Headers;

namespace NCoreUtils.Videos.WebService;

public class JsonStreamContent(Stream stream) : TypedStreamContent(stream, ApplicationJson)
{
    public static MediaTypeHeaderValue ApplicationJson { get; } = MediaTypeHeaderValue.Parse("application/json");
}
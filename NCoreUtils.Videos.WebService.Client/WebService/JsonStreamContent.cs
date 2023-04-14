using System.IO;
using System.Net.Http.Headers;

namespace NCoreUtils.Videos.WebService;

internal class JsonStreamContent : TypedStreamContent
{
    public static MediaTypeHeaderValue ApplicationJson { get; } = MediaTypeHeaderValue.Parse("application/json");

    public JsonStreamContent(Stream stream) : base(stream, ApplicationJson) { }
}
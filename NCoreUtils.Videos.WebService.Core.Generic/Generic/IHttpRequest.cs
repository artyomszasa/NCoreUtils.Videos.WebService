using System.IO;
using Microsoft.Extensions.Primitives;

namespace NCoreUtils.Videos.Generic;

public interface IHttpRequest
{
    string? ContentType { get; }

    Stream Body { get; }

    bool TryGetQueryParameter(string key, out StringValues values);
}
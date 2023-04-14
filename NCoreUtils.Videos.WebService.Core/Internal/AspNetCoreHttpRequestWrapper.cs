using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NCoreUtils.Videos.Internal;

public sealed class AspNetCoreHttpRequestWrapper : Generic.IHttpRequest
{
    private HttpRequest Request { get; }

    public string? ContentType => Request.ContentType;

    public Stream Body => Request.Body;

    public AspNetCoreHttpRequestWrapper(HttpRequest request)
        => Request = request ?? throw new ArgumentNullException(nameof(request));

    public bool TryGetQueryParameter(string key, out StringValues values)
        => Request.Query.TryGetValue(key, out values);
}
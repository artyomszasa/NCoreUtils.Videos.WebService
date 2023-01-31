using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NCoreUtils.Videos.WebService;

internal class TypedStreamContent : StreamContent
{
    public TypedStreamContent(Stream stream, MediaTypeHeaderValue contentType)
        : base(stream)
        => this.Headers.ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
}
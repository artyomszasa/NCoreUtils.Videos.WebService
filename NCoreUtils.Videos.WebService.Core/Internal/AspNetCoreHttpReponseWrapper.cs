using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.Videos.Internal;

public sealed class AspNetCoreHttpResponseWrapper : Generic.IHttpResponse
{
    private HttpResponse Response { get; }

    public string ContentType { set => Response.ContentType = value; }

    public long? ContentLength { set => Response.ContentLength = value; }

    public Stream Body => Response.Body;

    public AspNetCoreHttpResponseWrapper(HttpResponse response)
        => Response = response ?? throw new ArgumentNullException(nameof(response));
}
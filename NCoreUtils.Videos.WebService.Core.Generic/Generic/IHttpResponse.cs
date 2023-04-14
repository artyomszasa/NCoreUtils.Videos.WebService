using System.IO;

namespace NCoreUtils.Videos.Generic;

public interface IHttpResponse
{
    string ContentType { set; }

    long? ContentLength { set; }

    Stream Body { get; }
}
using System;
using System.Net;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteVideoCommunicationException : RemoteVideoException
{
    public HttpStatusCode HttpStatusCode { get; }

    public RemoteVideoCommunicationException(
        string endpoint,
        HttpStatusCode httpStatusCode,
        string description,
        Exception innerException)
        : base(endpoint, RemoteErrorCodes.CommunicationError, description, innerException)
        => HttpStatusCode = httpStatusCode;

    public RemoteVideoCommunicationException(
        string endpoint,
        HttpStatusCode httpStatusCode,
        string description)
        : base(endpoint, RemoteErrorCodes.CommunicationError, description)
        => HttpStatusCode = httpStatusCode;

#if !NET8_0_OR_GREATER

    protected RemoteVideoCommunicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(HttpStatusCode));

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(HttpStatusCode), (int)HttpStatusCode);
    }

#endif
}
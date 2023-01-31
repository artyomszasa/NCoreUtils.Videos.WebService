using System;
using System.Net;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class RemoteVideoCommunicationException : RemoteVideoException
{
    public HttpStatusCode HttpStatusCode { get; }

    protected RemoteVideoCommunicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(HttpStatusCode));

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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(HttpStatusCode), (int)HttpStatusCode);
    }
}
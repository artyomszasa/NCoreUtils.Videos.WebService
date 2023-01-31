using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class RemoteVideoException : VideoException, IRemoteVideoException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

    protected RemoteVideoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

    public RemoteVideoException(string endpoint, string errorCode, string description)
        : base(errorCode, description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteVideoException(string endpoint, string errorCode, string description, Exception innerException)
        : base(errorCode, description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }
}
using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class RemoteUnsupportedVideoTypeException : UnsupportedVideoTypeException, IRemoteVideoException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

    protected RemoteUnsupportedVideoTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

    public RemoteUnsupportedVideoTypeException(string endpoint, string videoType, string description)
        : base(videoType, description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteUnsupportedVideoTypeException(string endpoint, string videoType, string description, Exception innerException)
        : base(videoType, description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }
}
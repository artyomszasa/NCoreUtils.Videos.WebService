using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteUnsupportedVideoTypeException : UnsupportedVideoTypeException, IRemoteVideoException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

    public RemoteUnsupportedVideoTypeException(string endpoint, string videoType, string description)
        : base(videoType, description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteUnsupportedVideoTypeException(string endpoint, string videoType, string description, Exception innerException)
        : base(videoType, description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

#if !NET8_0_OR_GREATER

    protected RemoteUnsupportedVideoTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }

#endif
}
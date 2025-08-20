using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteInvalidVideoException : InvalidVideoException, IRemoteVideoException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

    public RemoteInvalidVideoException(string endpoint, string description)
        : base(description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteInvalidVideoException(string endpoint, string description, Exception innerException)
        : base(description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

#if !NET8_0_OR_GREATER

    protected RemoteInvalidVideoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }

#endif
}
using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class RemoteUnsupportedResizeModeException : UnsupportedResizeModeException, IRemoteVideoException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

    protected RemoteUnsupportedResizeModeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

    public RemoteUnsupportedResizeModeException(string endpoint, string resizeMode, int? width, int? height, string description)
        : base(resizeMode, width, height, description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteUnsupportedResizeModeException(string endpoint, string resizeMode, int? width, int? height, string description, Exception innerException)
        : base(resizeMode, width, height, description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }
}
using System;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteVideoConnectivityException : RemoteVideoException
{
    public SocketError SocketError { get; }

    public RemoteVideoConnectivityException(
        string endpoint,
        SocketError socketError,
        string description,
        Exception innerException)
        : base(endpoint, RemoteErrorCodes.ConnectivityError, description, innerException)
        => SocketError = socketError;

    public RemoteVideoConnectivityException(
        string endpoint,
        SocketError socketError,
        string description)
        : base(endpoint, RemoteErrorCodes.ConnectivityError, description)
        => SocketError = socketError;

#if !NET8_0_OR_GREATER

    protected RemoteVideoConnectivityException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => SocketError = (SocketError)info.GetInt32(nameof(SocketError));

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(SocketError), (int)SocketError);
    }

#endif
}
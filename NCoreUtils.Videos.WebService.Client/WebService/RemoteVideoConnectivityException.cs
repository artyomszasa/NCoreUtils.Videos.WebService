using System;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService
{
    [Serializable]
    public class RemoteVideoConnectivityException : RemoteVideoException
    {
        public SocketError SocketError { get; }

        protected RemoteVideoConnectivityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => SocketError = (SocketError)info.GetInt32(nameof(SocketError));

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(SocketError), (int)SocketError);
        }
    }
}
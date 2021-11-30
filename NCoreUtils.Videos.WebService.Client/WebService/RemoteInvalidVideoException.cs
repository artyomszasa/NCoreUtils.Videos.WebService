using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService
{
    [Serializable]
    public class RemoteInvalidImageException : InvalidVideoException, IRemoteVideoException
    {
        public string EndPoint { get; }

        public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

        protected RemoteInvalidImageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

        public RemoteInvalidImageException(string endpoint, string description)
            : base(description)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public RemoteInvalidImageException(string endpoint, string description, Exception innerException)
            : base(description, innerException)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(EndPoint), EndPoint);
        }
    }
}
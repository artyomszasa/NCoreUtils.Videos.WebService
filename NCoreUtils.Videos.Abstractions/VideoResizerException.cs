using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos
{
    [Serializable]
    public class VideoResizerException : Exception
    {
        public string ErrorType { get; }

        public VideoResizerException(string errorType, string message)
            : base(message)
        {
            ErrorType = errorType ?? ErrorCodes.GenericError;
        }

        public VideoResizerException(string errorType, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorType = errorType ?? ErrorCodes.GenericError;
        }

        protected VideoResizerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => ErrorType = info.GetString(nameof(ErrorType)) ?? string.Empty;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorType), ErrorType);
        }
    }
}
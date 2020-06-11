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
            ErrorType = errorType ?? "generic_error";
        }

        public VideoResizerException(string errorType, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorType = errorType ?? "generic_error";
        }

        protected VideoResizerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorType = info.GetString(nameof(ErrorType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorType), ErrorType);
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos
{
    /// <summary>
    /// Represents generic video processing error.
    /// </summary>
    [Serializable]
    public class VideoException : Exception
    {
        /// <summary>
        /// Related error code (see <see cref="ErrorCodes" />).
        /// </summary>
        public string ErrorCode { get; }

        protected VideoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => ErrorCode = info.GetString(nameof(ErrorCode));

        public VideoException(string errorCode, string description, Exception innerException)
            : base(description, innerException)
            => ErrorCode = errorCode;

        public VideoException(string errorCode, string description)
            : base(description)
            => ErrorCode = errorCode;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
        }
    }
}
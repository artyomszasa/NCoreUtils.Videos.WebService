using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos
{
    [Serializable]
    public class InvalidVideoException : VideoResizerException
    {
        const string DefaultMessage = "Unable to process the input.";

        public InvalidVideoException()
            : base("invalid_video", DefaultMessage)
        { }

        public InvalidVideoException(Exception innerException)
            : base("invalid_video", DefaultMessage, innerException)
        { }

        protected InvalidVideoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
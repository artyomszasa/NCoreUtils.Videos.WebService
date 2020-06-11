using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos
{
    [Serializable]
    public class NoVideoStreamException : VideoResizerException
    {
        public NoVideoStreamException() : base("no_video_stream", "No video stream found.") { }

        protected NoVideoStreamException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class NoVideoStreamException : VideoResizerException
{
    public NoVideoStreamException() : base("no_video_stream", "No video stream found.") { }

#if !NET8_0_OR_GREATER
    protected NoVideoStreamException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif
}
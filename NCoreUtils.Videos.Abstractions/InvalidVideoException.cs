using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos;

/// <summary>
/// Thrown if the supplied video is either unsupported or unprocessable.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class InvalidVideoException : VideoException
{
#if !NET8_0_OR_GREATER
    protected InvalidVideoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif

    public InvalidVideoException(string description)
        : base(ErrorCodes.InvalidVideo, description)
    { }

    public InvalidVideoException(string description, Exception innerException)
        : base(ErrorCodes.InvalidVideo, description, innerException)
    { }
}
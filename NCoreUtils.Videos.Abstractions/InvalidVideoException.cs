using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos;

/// <summary>
/// Thrown if the supplied video is either unsupported or unprocessable.
/// </summary>
[Serializable]
public class InvalidVideoException : VideoException
{
    protected InvalidVideoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public InvalidVideoException(string description)
        : base(ErrorCodes.InvalidVideo, description)
    { }

    public InvalidVideoException(string description, Exception innerException)
        : base(ErrorCodes.InvalidVideo, description, innerException)
    { }
}
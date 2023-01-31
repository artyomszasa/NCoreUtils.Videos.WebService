using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos;

/// <summary>
/// Represents error that has occured in video implementation.
/// </summary>
[Serializable]
public class InternalVideoException : VideoException
{
    public string InternalCode { get; }

    protected InternalVideoException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => InternalCode = info.GetString(nameof(InternalCode)) ?? string.Empty;

    public InternalVideoException(string internalCode, string description)
        : base(ErrorCodes.InternalError, description)
        => InternalCode = internalCode;

    public InternalVideoException(string internalCode, string description, Exception innerException)
        : base(ErrorCodes.InternalError, description, innerException)
        => InternalCode = internalCode;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(InternalCode), InternalCode);
    }
}
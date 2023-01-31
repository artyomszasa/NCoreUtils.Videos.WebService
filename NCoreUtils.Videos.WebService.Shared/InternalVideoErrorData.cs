using System;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class InternalVideoErrorData : VideoErrorData
{
    static readonly JsonEncodedText _keyInternalCode = JsonEncodedText.Encode(VideoErrorProperties.InternalCode);

    public string InternalCode { get; }

    public InternalVideoErrorData(string errorCode, string description, string internalCode)
        : base(errorCode, description)
        => InternalCode = internalCode;

    internal override void WriteTo(Utf8JsonWriter writer)
    {
        base.WriteTo(writer);
        writer.WriteString(_keyInternalCode, InternalCode);
    }
}
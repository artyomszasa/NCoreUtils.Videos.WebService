using System;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService;

[Serializable]
public class UnsupportedResizeModeData : VideoErrorData
{
    static readonly JsonEncodedText _keyResizeMode = JsonEncodedText.Encode(VideoErrorProperties.ResizeMode);

    static readonly JsonEncodedText _keyWidth = JsonEncodedText.Encode(VideoErrorProperties.Width);

    static readonly JsonEncodedText _keyHeight = JsonEncodedText.Encode(VideoErrorProperties.Height);

    public string? ResizeMode { get; }

    public int? Width { get; }

    public int? Height { get; }

    public UnsupportedResizeModeData(string errorCode, string description, string? resizeMode, int? width, int? height)
        : base(errorCode, description)
    {
        ResizeMode = resizeMode;
        Width = width;
        Height = height;
    }

    internal override void WriteTo(Utf8JsonWriter writer)
    {
        base.WriteTo(writer);
        if (!string.IsNullOrEmpty(ResizeMode))
        {
            writer.WriteString(_keyResizeMode, ResizeMode);
        }
        if (Width.HasValue)
        {
            writer.WriteNumber(_keyWidth, Width.Value);
        }
        if (Height.HasValue)
        {
            writer.WriteNumber(_keyHeight, Height.Value);
        }
    }
}
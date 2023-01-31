using System;

namespace NCoreUtils;

public sealed class MediaStreamInfo
{
    /// <summary>
    /// Index of the stream.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Stream type. One of the values defined in <see cref="MediaStreamTypes" />.
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// Codec short name if appliable.
    /// </summary>
    public string? Codec { get; }

    /// <summary>
    /// Duration of the stream.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Picture width of the video stream.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Picture height of the video stream.
    /// </summary>
    public int Height { get; }

    public MediaStreamInfo(int index, string? type, string? codec, TimeSpan duration, int width, int height)
    {
        Index = index;
        Type = type;
        Codec = codec;
        Duration = duration;
        Width = width;
        Height = height;
    }
}
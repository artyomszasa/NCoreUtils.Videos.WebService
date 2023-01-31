using System;
using NCoreUtils.Memory;

namespace NCoreUtils.Videos;

public class ResizeOptions : ISpanExactEmplaceable
{
    /// <summary>
    /// Desired output audio type. Defaults to the input audio type when not set.
    /// </summary>
    public string? AudioType { get; }

    /// <summary>
    /// Desired output video type. Defaults to the input video type when not set.
    /// </summary>
    public VideoSettings? VideoType { get; }

    /// <summary>
    /// Desired output video width. Defaults to the input video width when not set.
    /// </summary>
    public int? Width { get; }

    /// <summary>
    /// Desired output video height. Defaults to the input video height when not set.
    /// </summary>
    public int? Height { get; }

    /// <summary>
    /// Defines resizing mode. Defaults to <c>none</c>.
    /// </summary>
    public string? ResizeMode { get; }

    /// <summary>
    /// Desired quality of the output video. Server dependent default value is used when not set.
    /// </summary>
    public int? Quality { get; }

    /// <summary>
    /// Whether to perform any optimization on the output video. Server dependent default value is used when not
    /// set.
    /// </summary>
    public bool? Optimize { get; }

    /// <summary>
    /// Optional X coordinate of the weight point of the video.
    /// </summary>
    public int? WeightX { get; }

    /// <summary>
    /// Optional Y coordinate of the weight point of the video.
    /// </summary>
    public int? WeightY { get; }

    public ResizeOptions(
        string? audioType = default,
        VideoSettings? videoType = default,
        int? width = default,
        int? height = default,
        string? resizeMode = default,
        int? quality = default,
        bool? optimize = default,
        int? weightX = default,
        int? weightY = default)
    {
        AudioType = audioType;
        VideoType = videoType;
        Width = width;
        Height = height;
        ResizeMode = resizeMode;
        Quality = quality;
        Optimize = optimize;
        WeightX = weightX;
        WeightY = weightY;
    }

    internal int GetEmplaceBufferSize()
    {
        var size = 2;
        if (!string.IsNullOrEmpty(AudioType))
        {
            size += 2 + 9 + 3 + AudioType.Length;
        }
        if (VideoType is not null)
        {
            size += 2 + 9 + 3 + VideoType.GetEmplaceBufferSize();
        }
        if (Width.HasValue)
        {
            size += 2 + 5 + 3 + 21;
        }
        if (Height.HasValue)
        {
            size += 2 + 6 + 3 + 21;
        }
        if (!string.IsNullOrEmpty(ResizeMode))
        {
            size += 2 + 10 + 3 + ResizeMode.Length;
        }
        if (Quality.HasValue)
        {
            size += 2 + 7 + 3 + 21;
        }
        if (Optimize.HasValue)
        {
            size += 2 + 8 + 3 + 5;
        }
        if (WeightX.HasValue)
        {
            size += 2 + 7 + 3 + 21;
        }
        if (WeightY.HasValue)
        {
            size += 2 + 7 + 3 + 21;
        }
        return size;
    }

    int ISpanExactEmplaceable.GetEmplaceBufferSize()
        => GetEmplaceBufferSize();

    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = GetEmplaceBufferSize();
        return true;
    }

#if NET6_0_OR_GREATER
    string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider)
        => ToString();
#else
    public bool TryFormat(System.Span<char> destination, out int charsWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
        => TryEmplace(destination, out charsWritten);

    public int Emplace(Span<char> span)
    {
        if (TryEmplace(span, out var used))
        {
            return used;
        }
        throw new InsufficientBufferSizeException(span);
    }
#endif

    public bool TryEmplace(Span<char> span, out int used)
    {
        var builder = new SpanBuilder(span);
        var first = true;
        if (builder.TryAppend('[')
            && builder.TryAppendOption(ref first, nameof(AudioType), VideoType)
            && builder.TryAppendOption(ref first, nameof(VideoType), VideoType)
            && builder.TryAppendOption(ref first, nameof(Width), Width)
            && builder.TryAppendOption(ref first, nameof(Height), Height)
            && builder.TryAppendOption(ref first, nameof(ResizeMode), ResizeMode)
            && builder.TryAppendOption(ref first, nameof(Quality), Quality)
            && builder.TryAppendOption(ref first, nameof(Optimize), Optimize)
            && builder.TryAppendOption(ref first, nameof(WeightX), WeightX)
            && builder.TryAppendOption(ref first, nameof(WeightY), WeightY)
            && builder.TryAppend(']'))
        {
            used = builder.Length;
            return true;
        }
        used = default;
        return false;
    }

    public override string ToString()
        => this.ToStringUsingArrayPool();
}
using System;
using NCoreUtils.Memory;

namespace NCoreUtils.Videos.Logging;

public readonly struct ResizingVideoEntry(string videoSettings, bool isExplicit, int quality, bool optimize)
    : ISpanExactEmplaceable
{
    public static Func<ResizingVideoEntry, Exception?, string> Formatter { get; } =
        (entry, _) => entry.ToString();

    public string VideoSettings { get; } = videoSettings;

    public bool IsExplicit { get; } = isExplicit;

    public int Quality { get; } = quality;

    public bool Optimize { get; } = optimize;

    private int GetEmplaceBufferSize()
    {
        var minimumBufferSize = 56 + 12 + 13 + 21 + 5 + VideoSettings.Length;
        if (!IsExplicit)
        {
            minimumBufferSize += 11;
        }
        return minimumBufferSize;
    }

    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = GetEmplaceBufferSize();
        return true;
    }

    int ISpanExactEmplaceable.GetEmplaceBufferSize()
        => GetEmplaceBufferSize();

#if NET6_0_OR_GREATER
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
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
        used = default;
        if (!builder.TryAppend("Resizing video with computed options [VideoSettings = ")) { return false; }
        if (!builder.TryAppend(VideoSettings)) { return false; }
        if (!IsExplicit)
        {
            if (!builder.TryAppend(" (implicit)")) { return false; }
        }
        if (builder.TryAppend(", Quality = ")
            && builder.TryAppend(Quality)
            && builder.TryAppend(", Optimize = ")
            && builder.TryAppend(Optimize ? "true" : "false")
            && builder.TryAppend("]."))
        {
            used = builder.Length;
            return true;
        }
        return false;
    }

    public override string ToString()
        => Emplacer.ToStringUsingArrayPool(this);
}
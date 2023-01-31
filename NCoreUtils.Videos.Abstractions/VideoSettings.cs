using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NCoreUtils.Memory;

namespace NCoreUtils.Videos;

public abstract class VideoSettings
    : ISpanExactEmplaceable
#if NET7_0_OR_GREATER
    , ISpanParsable<VideoSettings>
#endif
{
    public static NCoreUtils.Memory.IEmplacer<VideoSettings> Emplacer { get; }
            = new NCoreUtils.Memory.SpanEmplaceableEmplacer<VideoSettings>();

    public static VideoSettings Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Unable to parse \"{new string(s)}\" as video settings.");

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out VideoSettings result)
    {
        if (!s.StartsWith("x264"))
        {
            result = default;
            return false;
        }
        if (MemoryExtensions.Equals(s, "x264", StringComparison.InvariantCulture))
        {
            result = X264Settings.Default;
            return true;
        }
        if (s.Length > 4 && s[4] != ':')
        {
            result = default;
            return false;
        }
        ReadOnlySpan<char> arg;
        var rem = s[5..];
        long? bitRate = default;
        string? pixelFormat = default;
        string? preset = default;
        while (rem.Length != 0)
        {
            var sepIndex = rem.IndexOf(':');
            if (-1 == sepIndex)
            {
                arg = rem;
                rem = ReadOnlySpan<char>.Empty;
            }
            else
            {
                arg = rem[..sepIndex];
                rem = rem[(sepIndex + 1)..];
            }
            var eqIndex = arg.IndexOf('=');
            if (-1 == eqIndex)
            {
                result = default;
                return false;
            }
            var argName = arg[..eqIndex];
            var argValue = arg[(eqIndex + 1)..];
            if (MemoryExtensions.Equals(argName, "b", StringComparison.InvariantCulture))
            {
                if (!long.TryParse(argValue, NumberStyles.None, CultureInfo.InvariantCulture, out var b))
                {
                    result = default;
                    return false;
                }
                bitRate = b;
            }
            else if (MemoryExtensions.Equals(argName, "pixfmt", StringComparison.InvariantCulture))
            {
                pixelFormat = new(argValue);
            }
            else if (MemoryExtensions.Equals(argName, "preset", StringComparison.InvariantCulture))
            {
                preset = new(argValue);
            }
        }
        result = new X264Settings(bitRate, pixelFormat, preset);
        return true;
    }

    public static VideoSettings Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out VideoSettings result)
        => TryParse(s.AsSpan(), provider, out result);

    public string Codec { get; }

    public long? BitRate { get; }

    public string? PixelFormat { get; }

    protected VideoSettings(string codec, long? bitRate, string? pixelFormat)
    {
        Codec = codec ?? throw new ArgumentNullException(nameof(codec));
        BitRate = bitRate;
        PixelFormat = pixelFormat;
    }

    public abstract int GetEmplaceBufferSize();

    public abstract bool TryEmplace(Span<char> span, out int used);

    public override string ToString()
        => this.ToStringUsingArrayPool();

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

#if !NET6_0_OR_GREATER

    int ISpanEmplaceable.Emplace(System.Span<char> span)
        => TryEmplace(span, out var used)
            ? used
            : throw new InsufficientBufferSizeException(span, GetEmplaceBufferSize());

    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = GetEmplaceBufferSize();
        return true;
    }

    bool ISpanEmplaceable.TryFormat(System.Span<char> destination, out int charsWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
        => TryEmplace(destination, out charsWritten);

#endif
}

public sealed class X264Settings : VideoSettings
{
    public static X264Settings Default { get; } = new(default, default, default);

    public string? Preset { get; }

    public X264Settings(long? bitRate, string? pixelFormat, string? preset)
        : base("x264", bitRate, pixelFormat)
        => Preset = preset;

    public override int GetEmplaceBufferSize()
    {
        var size = 4; // "x264".Length
        if (BitRate is long)
        {
            size += 1 + 2 /* "b=".Length */ + 20;
        }
        if (!string.IsNullOrEmpty(PixelFormat))
        {
            size += 1 + 7 /* "pixfmt=".Length */ + PixelFormat.Length;
        }
        if (!string.IsNullOrEmpty(Preset))
        {
            size += 1 + 7 /* "preset=".Length */ + Preset.Length;
        }
        return size;
    }

    public override bool TryEmplace(Span<char> span, out int used)
    {
        var builder = new SpanBuilder(span);
        used = 0;
        if (!builder.TryAppend("x264")) { return false; }
        if (BitRate is long bitRate)
        {
            if (!builder.TryAppend(":b=") || !builder.TryAppend(bitRate)) { return false; }
        }
        if (!string.IsNullOrEmpty(PixelFormat))
        {
            if (!builder.TryAppend(":pixfmt=") || !builder.TryAppend(PixelFormat)) { return false; }
        }
        if (!string.IsNullOrEmpty(Preset))
        {
            if (!builder.TryAppend(":preset=") || !builder.TryAppend(Preset)) { return false; }
        }
        used = builder.Length;
        return true;
    }
}
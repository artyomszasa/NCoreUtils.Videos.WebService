using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NCoreUtils.Videos
{
    public class VideoOptions
    {
        static private StringBuilder AppendProp(StringBuilder builder, ref bool first, string name, string value)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }
            builder.Append(name);
            builder.Append('=');
            builder.Append(value);
            return builder;
        }

        static private StringBuilder AppendProp(StringBuilder builder, ref bool first, string name, int value)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }
            builder.Append(name);
            builder.Append('=');
            builder.Append(value);
            return builder;
        }

        static private bool TryAppendProp(ref SpanBuilder builder, ref bool first, string name, string value)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                if (!builder.TryAppend(", ")) { return false; }
            }
            if (!builder.TryAppend(name)) { return false; }
            if (!builder.TryAppend('=')) { return false; }
            if (!builder.TryAppend(value)) { return false; }
            return true;
        }

        static private bool TryAppendProp(ref SpanBuilder builder, ref bool first, string name, int value)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                if (!builder.TryAppend(", ")) { return false; }
            }
            if (!builder.TryAppend(name)) { return false; }
            if (!builder.TryAppend('=')) { return false; }
            if (!builder.TryAppend(value)) { return false; }
            return true;
        }

        public string? VideoType { get; }

        public int? Width { get; }

        public int? Height { get; }

        public int? Quality { get; }

        public string? Watermark { get;}

        public VideoOptions(string? videoType, int? width, int? height, int? quality, string? watermark)
        {
            VideoType = videoType;
            Width = width;
            Height = height;
            Quality = quality;
            Watermark = watermark;
        }

        private bool TryToStringNoAlloc([NotNullWhen(true)] out string? result)
        {
            Span<char> buffer = stackalloc char[2048];
            var builder = new SpanBuilder(buffer);
            result = default;
            if (!builder.TryAppend('[')) { return false; }
            var first = true;
            if (!(VideoType is null))
            {
                if (!TryAppendProp(ref builder, ref first, nameof(VideoType), VideoType)) { return false; }
            }
            if (Width.HasValue)
            {
                if (!TryAppendProp(ref builder, ref first, nameof(Width), Width.Value)) { return false; }
            }
            if (Height.HasValue)
            {
                if (!TryAppendProp(ref builder, ref first, nameof(Height), Height.Value)) { return false; }
            }
            if (Quality.HasValue)
            {
                if (!TryAppendProp(ref builder, ref first, nameof(Quality), Quality.Value)) { return false; }
            }
            if (!(Watermark is null))
            {
                if (!TryAppendProp(ref builder, ref first, nameof(Watermark), Watermark)) { return false; }
            }
            if (!builder.TryAppend(']')) { return false; }
            result = builder.ToString();
            return true;
        }

        public override string ToString()
        {
            if (TryToStringNoAlloc(out var result))
            {
                return result;
            }
            var builder = new StringBuilder();
            builder.Append('[');
            var first = true;
            if (!(VideoType is null))
            {
                AppendProp(builder, ref first, nameof(VideoType), VideoType);
            }
            if (Width.HasValue)
            {
                AppendProp(builder, ref first, nameof(Width), Width.Value);
            }
            if (Height.HasValue)
            {
                AppendProp(builder, ref first, nameof(Height), Height.Value);
            }
            if (Quality.HasValue)
            {
                AppendProp(builder, ref first, nameof(Quality), Quality.Value);
            }
            if (!(Watermark is null))
            {
                AppendProp(builder, ref first, nameof(Watermark), Watermark);
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
using System;
using System.Buffers;

namespace NCoreUtils.Videos.WebService
{
    static class SpanBuilderExtensions
    {
        public static ref SpanBuilder AppendQ(this ref SpanBuilder builder, scoped ref bool first, string name, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return ref builder;
            }
            if (first)
            {
                first = false;
                builder.Append('?');
            }
            else
            {
                builder.Append('&');
            }
            builder.Append(name);
            builder.Append('=');
#if NET7_0_OR_GREATER
            builder.AppendUriEscaped(value);
#else
            builder.Append(Uri.EscapeDataString(value));
#endif
            return ref builder;
        }

        public static ref SpanBuilder AppendQ(this ref SpanBuilder builder, scoped ref bool first, string name, VideoSettings? value)
        {
            if (value is null)
            {
                return ref builder;
            }
            if (first)
            {
                first = false;
                builder.Append('?');
            }
            else
            {
                builder.Append('&');
            }
            var buffer = ArrayPool<char>.Shared.Rent(value.GetEmplaceBufferSize());
            try
            {
                var size = VideoSettings.Emplacer.Emplace(value, buffer);
                builder.Append(name);
                builder.Append('=');
#if NET7_0_OR_GREATER
                builder.AppendUriEscaped(buffer.AsSpan(0, size));
#else
                builder.Append(Uri.EscapeDataString(new(buffer, 0, size)));
#endif
                return ref builder;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        public static ref SpanBuilder AppendQ(this ref SpanBuilder builder, scoped ref bool first, string name, int? value)
        {
            if (!value.HasValue)
            {
                return ref builder;
            }
            var escapedValue = value.Value.ToString();
            if (first)
            {
                first = false;
                builder.Append('?');
            }
            else
            {
                builder.Append('&');
            }
            builder.Append(name);
            builder.Append('=');
            builder.Append(escapedValue);
            return ref builder;
        }

        public static ref SpanBuilder AppendQ(this ref SpanBuilder builder, scoped ref bool first, string name, bool? value)
        {
            if (!value.HasValue)
            {
                return ref builder;
            }
            var escapedValue = value.Value ? "true" : "false";
            if (first)
            {
                first = false;
                builder.Append('?');
            }
            else
            {
                builder.Append('&');
            }
            builder.Append(name);
            builder.Append('=');
            builder.Append(escapedValue);
            return ref builder;
        }
    }
}
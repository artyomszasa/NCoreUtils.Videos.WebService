using System.Runtime.CompilerServices;

namespace NCoreUtils.Videos
{
    internal static class SpanBuilderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAppendOption(this ref SpanBuilder builder, ref bool first, string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(", ")) { return false; }
                }
                return builder.TryAppend(key)
                    && builder.TryAppend(" = ")
                    && builder.TryAppend(value!);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAppendOption(this ref SpanBuilder builder, ref bool first, string key, int? value)
        {
            if (value.HasValue)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(", ")) { return false; }
                }
                return builder.TryAppend(key)
                    && builder.TryAppend(" = ")
                    && builder.TryAppend(value.Value);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAppendOption(this ref SpanBuilder builder, ref bool first, string key, bool? value)
        {
            if (value.HasValue)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(", ")) { return false; }
                }
                return builder.TryAppend(key)
                    && builder.TryAppend(" = ")
                    && builder.TryAppend(value.Value);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAppendOption<T>(this ref SpanBuilder builder, ref bool first, string key, T? value)
            where T : class
        {
            if (value is not null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(", ")) { return false; }
                }
                return builder.TryAppend(key)
                    && builder.TryAppend(" = ")
                    && builder.TryAppend(value);
            }
            return true;
        }
    }
}
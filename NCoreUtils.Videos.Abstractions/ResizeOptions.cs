using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NCoreUtils.Memory;

namespace NCoreUtils.Videos
{
    public class ResizeOptions : IEmplaceable<ResizeOptions>
    {
        /// <summary>
        /// Desired output audio type. Defaults to the input audio type when not set.
        /// </summary>
        public string? AudioType { get; }

        /// <summary>
        /// Desired output video type. Defaults to the input video type when not set.
        /// </summary>
        public string? VideoType { get; }

        /// <summary>
        /// Desired output video width. Defaults to the input image width when not set.
        /// </summary>
        public int? Width { get; }

        /// <summary>
        /// Desired output video height. Defaults to the input image height when not set.
        /// </summary>
        public int? Height { get; }

        /// <summary>
        /// Defines resizing mode. Defaults to <c>none</c>.
        /// </summary>
        public string? ResizeMode { get; }

        /// <summary>
        /// Desired quality of the output image. Server dependent default value is used when not set.
        /// </summary>
        public int? Quality { get; }

        /// <summary>
        /// Whether to perform any optimization on the output image. Server dependent default value is used when not
        /// set.
        /// </summary>
        public bool? Optimize { get; }

        /// <summary>
        /// Optional X coordinate of the weight point of the image.
        /// </summary>
        public int? WeightX { get; }

        /// <summary>
        /// Optional Y coordinate of the weight point of the image.
        /// </summary>
        public int? WeightY { get; }

        public ResizeOptions(
            string? audioType = default,
            string? videoType = default,
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryToStringOnStack([NotNullWhen(true)] out string? result)
        {
            Span<char> buffer = stackalloc char[4 * 1024];
            if (TryEmplace(buffer, out var size))
            {
                result = buffer[..size].ToString();
                return true;
            }
            result = default;
            return false;
        }

        public override string ToString()
        {
            if (TryToStringOnStack(out var result))
            {
                return result;
            }
            using var memoryBuffer = MemoryPool<char>.Shared.Rent(32 * 1024);
            var buffer = memoryBuffer.Memory.Span;
            var size = Emplace(buffer);
            return buffer[..size].ToString();
        }

        public int Emplace(Span<char> span)
        {
            if (TryEmplace(span, out var used))
            {
                return used;
            }
            throw new ArgumentException("Insufficient buffer size.", nameof(span));
        }

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
    }
}
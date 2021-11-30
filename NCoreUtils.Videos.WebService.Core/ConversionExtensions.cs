using Xabe.FFmpeg;

namespace NCoreUtils.Videos.WebService
{
    public static class ConversionExtensions
    {
        public static IConversion AddStreamIfNotNull(this IConversion conversion, IAudioStream? stream)
            => stream is null
                ? conversion
                : conversion.AddStream(stream);
    }
}
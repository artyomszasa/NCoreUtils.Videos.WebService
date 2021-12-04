using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.WebService
{
    internal static class ErrorSerialization
    {
        public static JsonSerializerOptions Options { get; }

        static ErrorSerialization()
        {
            Options = new JsonSerializerOptions();
            Options.Converters.Add(new VideoErrorConverter());
        }

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Potentially serializable types referenced directly within converter.")]
#endif
        public static ValueTask<VideoErrorData?> DeserializeVideoErrorDataAsync(System.IO.Stream stream, CancellationToken cancellationToken)
            => JsonSerializer.DeserializeAsync<VideoErrorData>(stream, Options, cancellationToken);

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Potentially serializable types referenced directly within converter.")]
#endif
        public static Task SerializeVideoErrorDataAsync(System.IO.Stream stream, VideoErrorData data, CancellationToken cancellationToken)
            => JsonSerializer.SerializeAsync(stream, data, Options, cancellationToken);

    }
}
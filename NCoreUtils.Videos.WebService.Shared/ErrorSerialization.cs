using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.WebService;

internal static class ErrorSerialization
{
    public static ValueTask<VideoErrorData?> DeserializeVideoErrorDataAsync(System.IO.Stream stream, CancellationToken cancellationToken)
        => JsonSerializer.DeserializeAsync(stream, VideoErrorSerializerContext.Default.VideoErrorData, cancellationToken);

    public static Task SerializeVideoErrorDataAsync(System.IO.Stream stream, VideoErrorData data, CancellationToken cancellationToken)
        => JsonSerializer.SerializeAsync(stream, data, VideoErrorSerializerContext.Default.VideoErrorData, cancellationToken);
}
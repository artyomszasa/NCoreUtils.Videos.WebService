using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos;

public interface IVideoResizer
{
    ValueTask ResizeAsync(
        IReadableResource source,
        IWritableResource destination,
        ResizeOptions options,
        CancellationToken cancellationToken = default
    );

    ValueTask CreateThumbnailAsync(
        IReadableResource source,
        IWritableResource destination,
        ResizeOptions options,
        CancellationToken cancellationToken = default
    );
}
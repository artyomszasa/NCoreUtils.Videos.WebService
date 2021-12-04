using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos
{
    public interface IVideoResizer
    {
        ValueTask ResizeAsync(
            IVideoSource source,
            IVideoDestination destination,
            ResizeOptions options,
            CancellationToken cancellationToken = default
        );

        ValueTask CreateThumbnailAsync(
            IVideoSource source,
            IVideoDestination destination,
            ResizeOptions options,
            CancellationToken cancellationToken = default
        );
    }
}
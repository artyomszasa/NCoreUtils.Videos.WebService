using System;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Images;
using NCoreUtils.Videos;

namespace NCoreUtils
{
    public interface IVideoResizer
    {
        ValueTask ResizeAsync(IVideoSource source, IVideoDestination destination, VideoOptions options, CancellationToken cancellationToken);

        ValueTask CreateThumbnailAsync(IVideoSource source, IImageDestination destination, ResizeOptions options, CancellationToken cancellationToken);
    }
}
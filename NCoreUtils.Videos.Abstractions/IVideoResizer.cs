using System;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Images;
using NCoreUtils.Videos;

namespace NCoreUtils
{
    public interface IVideoResizer
    {
        Task ResizeAsync(Uri source, Uri destination, VideoOptions options, CancellationToken cancellationToken);

        Task Thumbnail(Uri source, Uri destination, ResizeOptions options, CancellationToken cancellationToken);
    }
}
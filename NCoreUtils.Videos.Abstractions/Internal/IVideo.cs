using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal
{
    public interface IVideo : IAsyncDisposable
    {
        Size Size { get; }

        int Rotation { get; }

        string VideoType { get; }

        ValueTask CropAsync(Rectangle rect, CancellationToken cancellationToken = default);

        ValueTask<VideoInfo> GetVideoInfoAsync(CancellationToken cancellationToken = default);

        // ValueTask NormalizeAsync(CancellationToken cancellationToken = default);

        ValueTask ResizeAsync(Size size, CancellationToken cancellationToken = default);

        ValueTask WriteToAsync(Stream stream, string imageType, int quality = 85, bool optimize = true, CancellationToken cancellationToken = default);

        ValueTask WriteThumbnailAsync(Stream stream, TimeSpan captureTime, CancellationToken cancellationToken = default);
    }
}
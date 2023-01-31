using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal;

public interface IVideo : IAsyncDisposable
{
    Size Size { get; }

    int Rotation { get; }

    string? VideoCodec { get; }

    string? AudioCodec { get; }

    ValueTask<VideoInfo> GetVideoInfoAsync(CancellationToken cancellationToken = default);

    ValueTask WriteToAsync(
        Stream stream,
        IReadOnlyList<VideoTransformation> transformations,
        VideoSettings? videoSettings,
        string? audioType,
        int quality = 85,
        bool optimize = true,
        CancellationToken cancellationToken = default
    );

    ValueTask WriteThumbnailAsync(Stream stream, TimeSpan captureTime, CancellationToken cancellationToken = default);
}
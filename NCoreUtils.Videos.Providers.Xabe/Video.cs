using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Videos.Internal;
using Xabe.FFmpeg;

namespace NCoreUtils.Videos.Xabe;

public sealed class Video : IVideo
{
    private int _isDisposed;

    private bool LeaveFile { get; }

    public string Path { get; }

    public IMediaInfo MediaInfo { get; }

    public IVideoStream? VideoStream { get; }

    public IAudioStream? AudioStream { get; }

    public Size Size => VideoStream is null
        ? default
        : new(VideoStream.Width, VideoStream.Height);

    public string? VideoCodec => VideoStream?.Codec;

    public string? AudioCodec => AudioStream?.Codec;

    public int Rotation { get; }

    internal Video(
        string path,
        IMediaInfo mediaInfo,
        IVideoStream? videoStream,
        IAudioStream? audioStream,
        int rotation,
        bool leaveFile = false)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        MediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
        VideoStream = videoStream;
        AudioStream = audioStream;
        Rotation = rotation;
        LeaveFile = leaveFile;
    }

    public ValueTask<VideoInfo> GetVideoInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = new VideoInfo(
            MediaInfo.Duration,
            MediaInfo.Streams.Select(stream => stream switch
            {
                IVideoStream videoStream => new MediaStreamInfo(
                    index: videoStream.Index,
                    type: MediaStreamTypes.Video,
                    codec: videoStream.Codec,
                    duration: videoStream.Duration,
                    width: Rotation == 90 || Rotation == 270 ? videoStream.Width : videoStream.Height,
                    height: Rotation == 90 || Rotation == 270 ? videoStream.Height : videoStream.Width
                ),
                IAudioStream audioStream => new MediaStreamInfo(
                    index: audioStream.Index,
                    type: MediaStreamTypes.Audio,
                    codec: audioStream.Codec,
                    duration: audioStream.Duration,
                    width: default,
                    height: default
                ),
                ISubtitleStream subtitleStream => new MediaStreamInfo(
                    index: subtitleStream.Index,
                    type: MediaStreamTypes.Other,
                    codec: subtitleStream.Codec,
                    duration: default,
                    width: default,
                    height: default
                ),
                _ => new MediaStreamInfo(
                    index: stream.Index,
                    type: MediaStreamTypes.Other,
                    codec: stream.Codec,
                    duration: default,
                    width: default,
                    height: default
                )
            }).ToList(),
            VideoStream?.Index,
            AudioStream?.Index
        );
        return new(result);
    }

    public ValueTask WriteToAsync(
        Stream stream,
        IReadOnlyList<VideoTransformation> transformations,
        VideoSettings? videoType,
        string? audioType,
        int quality = 85,
        bool optimize = true,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }


    public ValueTask DisposeAsync()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            if (!LeaveFile)
            {
                File.Delete(Path);
            }
        }
        return default;
    }

    public async ValueTask WriteThumbnailAsync(Stream stream, TimeSpan captureTime, CancellationToken cancellationToken = default)
    {
        if (VideoStream is null)
        {
            throw new NoVideoStreamException();
        }
        var vstream = VideoStream
                .SetCodec(global::Xabe.FFmpeg.VideoCodec.png)
                .SetOutputFramesCount(1)
                .SetSeek(captureTime);

        if (Rotation == 90 || Rotation == 270)
        {
            vstream = vstream.SetSize(vstream.Height, vstream.Width);
        }
        var tmpOutputPath = System.IO.Path.GetTempFileName();
        try
        {
            var conversion = FFmpeg.Conversions.New()
                .AddStream(vstream)
                .SetOutput(tmpOutputPath)
                .SetOverwriteOutput(true)
                .UseMultiThread(true)
                .SetPreset(ConversionPreset.Slow);
            await conversion.Start(cancellationToken).ConfigureAwait(false);
            await using var thumbnail = new FileStream(tmpOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 32 * 1024, FileOptions.SequentialScan | FileOptions.Asynchronous);
            await thumbnail.CopyToAsync(stream, 32 * 1024, cancellationToken);
        }
        finally
        {
            if (File.Exists(tmpOutputPath))
            {
                File.Delete(tmpOutputPath);
            }
        }
    }
}
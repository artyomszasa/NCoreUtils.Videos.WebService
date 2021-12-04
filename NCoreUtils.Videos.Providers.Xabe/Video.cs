using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Videos.Internal;
using Xabe.FFmpeg;

namespace NCoreUtils.Videos.Xabe
{
    public sealed class Video : IVideo
    {
        private int _isDisposed;

        private bool LeaveFile { get; }

        public string Path { get; }

        public IVideoStream VideoStream { get; }

        public IAudioStream? AudioStream { get; }

        public Size Size => new(VideoStream.Width, VideoStream.Height);

        public string VideoType => VideoStream.Codec;

        public int Rotation { get; }

        internal Video(string path, IVideoStream videoStream, IAudioStream? audioStream, int rotation, bool leaveFile = false)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            VideoStream = videoStream ?? throw new ArgumentNullException(nameof(videoStream));
            AudioStream = audioStream;
            Rotation = rotation;
            LeaveFile = leaveFile;
        }

        public ValueTask CropAsync(Rectangle rect, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<VideoInfo> GetVideoInfoAsync(CancellationToken cancellationToken = default)
        {
            var (w, h) = Rotation == 90 || Rotation == 270
                ? (VideoStream.Width, VideoStream.Height)
                : (VideoStream.Height, VideoStream.Width);
            return new ValueTask<VideoInfo>(new VideoInfo(w, h));
        }

        public ValueTask ResizeAsync(Size size, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask WriteToAsync(Stream stream, string imageType, int quality = 85, bool optimize = true, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
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
            var vstream = VideoStream
                    .SetCodec(VideoCodec.png)
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
}
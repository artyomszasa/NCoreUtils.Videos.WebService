using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.Internal;
using Xabe.FFmpeg;

namespace NCoreUtils.Videos.Xabe
{
    public sealed class VideoProvider : IVideoProvider
    {
        private static async Task<IMediaInfo> GetMediaInfo(string path)
        {
            try
            {
                return await FFmpeg.GetMediaInfo(path).ConfigureAwait(false);
            }
            catch (Exception exn)
            {
                throw new InvalidVideoException("Unable to get media info.", exn);
            }
        }

        public ILogger Logger { get; }

        public VideoProvider(ILogger<VideoProvider> logger)
            => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async ValueTask<IVideo> FromStreamAsync(Stream source, CancellationToken cancellationToken = default)
        {
            var tmpVideoPath = Path.GetTempFileName();
            {
                await using var tmpStream = new FileStream(tmpVideoPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 32 * 1024, true);
                await source.CopyToAsync(tmpStream, 32 * 1024, cancellationToken);
            }
            var mediaInfo = await GetMediaInfo(tmpVideoPath).ConfigureAwait(false);
            if (!mediaInfo.VideoStreams.TryGetFirst(out var videoStream))
            {
                throw new NoVideoStreamException();
            }
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            int rotation;
            try
            {
                var rotStr = await Probe.New()
                    .Start($"-v error -select_streams v:0 -show_entries stream_tags=rotate -of csv=p=0 {tmpVideoPath}", cancellationToken);
                rotation = int.TryParse(rotStr?.Trim(), out var r) ? r : 0;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to extract rotation [Path = {Path}].", tmpVideoPath);
                rotation = 0;
            }
            return new Video(tmpVideoPath, mediaInfo, videoStream, audioStream, rotation);
        }
    }
}
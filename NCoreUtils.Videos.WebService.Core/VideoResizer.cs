using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using NCoreUtils.IO;
using Xabe.FFmpeg;

namespace NCoreUtils.Videos.WebService
{
    public class VideoResizer : IVideoResizer
    {
        private sealed class WatermarkStream : IStream
        {
            public string Path { get; }

            public int Index => 0;

            public string Codec => "png_pipe";

            public StreamType StreamType => StreamType.Video;

            public string Build()
            {
                return " -filter_complex \"[0:v][1:0]overlay=10:main_h-overlay_h-10 \" ";
            }

            public string BuildInputArguments()
            {
                return string.Empty;
            }

            public IEnumerable<string> GetSource()
            {
                yield return Path;
            }

            public WatermarkStream(string path)
            {
                Path = path;
            }
        }

        static VideoResizer()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                FFmpeg.SetExecutablesPath("/usr/bin");
            }
        }

        private readonly ILogger _logger;

        private readonly IImageResizer _imageResizer;

        public VideoResizer(
            ILogger<IVideoResizer> logger,
            IImageResizer imageResizer)
        {
            _logger = logger;
            _imageResizer = imageResizer ?? throw new ArgumentNullException(nameof(imageResizer));
        }

        private int RoundTodivisibleByTwo(double number)
        {
            var n = Convert.ToInt32(Math.Ceiling(number));
            while (n % 2 > 0)
            {
                n++;
            }
            return n;
        }

        private async Task<IMediaInfo> GetMediaInfo(string path)
        {
            try
            {
                return await FFmpeg.GetMediaInfo(path).ConfigureAwait(false);
            }
            catch (Exception exn)
            {
                throw new InvalidVideoException(exn);
            }
        }

        public async Task ResizeAsync(IStreamProducer producer, IStreamConsumer consumer, VideoOptions options, CancellationToken cancellationToken)
        {
            var width = options.Width;
            var height = options.Height;
            // FIXME: validation...
            if (!width.HasValue && !height.HasValue)
            {
                throw new ArgumentNullException(nameof(width));
            }
            // await FFmpeg.GetLatestVersion().ConfigureAwait(false);
            var inputFilename = Path.GetTempFileName();
            var outputFilename = Path.ChangeExtension(inputFilename, "mp4");
            string? watermarkFilename = default;
            try
            {
                var stopwatch = new Stopwatch();
                _logger.LogDebug("Start reading input into local buffer [Path = {0}].", inputFilename);
                stopwatch.Restart();
                using (var stream = File.Create(inputFilename, 64 * 1024, FileOptions.Asynchronous))
                {
                    await producer.ProduceAsync(stream, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
                stopwatch.Stop();
                _logger.LogDebug("Reading input into local buffer took {0}ms [Path = {1}].", stopwatch.ElapsedMilliseconds, inputFilename);
                var mediaInfo = await GetMediaInfo(inputFilename).ConfigureAwait(false);

                if (!mediaInfo.VideoStreams.TryGetFirst(out var videoStream))
                {
                    throw new NoVideoStreamException();
                }
                _logger.LogDebug("Processing video with options {0}, [Path = {1}].", options, inputFilename);
                var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

                var rotation = 0;
                try
                {
                    var rotStr = await Probe.New()
                        //.Start($"-v error -select_streams v:0 -show_entries stream=width,height:stream_tags=rotate -of csv=p=0 {inputFile}"))
                        .Start($"-v error -select_streams v:0 -show_entries stream_tags=rotate -of csv=p=0 {inputFilename}");
                    int.TryParse(rotStr?.Trim(), out rotation);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to extract rotation [Path = {0}].", inputFilename);
                }
                var (vw, vh) = rotation == 90 || rotation == 270
                    ? (videoStream.Height, videoStream.Width)
                    : (videoStream.Width, videoStream.Height);

                var aspectRatio = (double)vw / vh;
                int w = 0;
                int h = 0;

                if (width.HasValue && height.HasValue)
                {
                    w = Math.Min(width.Value, vw);
                    h = Math.Min(height.Value, vh);
                }
                else if (width.HasValue)
                {
                    w = Math.Min(width.Value, vw);
                    h = RoundTodivisibleByTwo(w / aspectRatio);
                }
                else if (height.HasValue)
                {
                    h = Math.Min(height.Value, vh);
                    w = RoundTodivisibleByTwo(h * aspectRatio);
                }
                if (w != vw || h != vh)
                {
                    _logger.LogDebug("Performing video resizing: {0}x{1} => {2}x{3}.", vw, vh, w, h);
                }
                else
                {
                    _logger.LogDebug("Performing no video resizing (original size: {0}x{1}).", w, h);
                }

                audioStream?.SetCodec(AudioCodec.aac);

                videoStream
                    .SetSize(w, h)
                    .SetCodec(VideoCodec.h264)
                    .SetBitrate(1600000);
                // if (rotation == 90)
                // {
                //     videoStream.Rotate(RotateDegrees.Clockwise);
                // }

                var conversion = FFmpeg.Conversions.New()
                    .AddStream(videoStream)
                    .AddStreamIfNotNull(audioStream);

                if (!string.IsNullOrEmpty(options.Watermark))
                {
                    watermarkFilename = Path.ChangeExtension(inputFilename, "png");
                    var watermarkUri = new Uri(options.Watermark);
                    /*
                    // FIXME: validation
                    {
                        await using var watermarkProducer = new GCSProducer(_storageClient, watermarkUri.Host, watermarkUri.LocalPath.Trim('/'));
                        await using var watermarkStoreStream = new FileStream(watermarkFilename, FileMode.Create, FileAccess.Write, FileShare.None, 16 * 1024, true);
                        await using var watermarkStore = StreamConsumer.ToStream(watermarkStoreStream);
                        await watermarkProducer.ConsumeAsync(watermarkStore);
                    }
                    */

                    // BUGBUGBUG
                    //videoStream.SetWatermark(watermarkFilename, Position.BottomRight);
                    conversion.AddStream(new WatermarkStream(watermarkFilename));
                    _logger.LogDebug("Added video watermark [Video = {0}, Watermark = {1}].", inputFilename, watermarkUri);
                }

                conversion
                    .SetOutput(outputFilename)
                    .SetOverwriteOutput(true)
                    .UseMultiThread(true)
                    .SetPreset(ConversionPreset.Slow);

                conversion.OnProgress += (sender, args) =>
                {
                    _logger.LogDebug($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] {inputFilename}");
                };
                _logger.LogDebug("Start video conversion [Source = {0}, Target = {1}].", inputFilename, outputFilename);
                stopwatch.Restart();
                var result = await conversion.Start(cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                _logger.LogInformation("Video conversion took {0}ms [Arguments: {1}].", stopwatch.ElapsedMilliseconds, result.Arguments);
                _logger.LogDebug("Start writing output from local buffer [Path = {0}].", outputFilename);
                stopwatch.Restart();
                using (var stream = new FileStream(outputFilename, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await consumer.ConsumeAsync(stream, cancellationToken);
                }
                stopwatch.Stop();
                _logger.LogDebug("Writing output from local buffer took {0}ms [Path = {0}].", stopwatch.ElapsedMilliseconds, outputFilename);
            }
            catch (Xabe.FFmpeg.Exceptions.ConversionException exn)
            {
                _logger.LogError("Failed operation commandline: {0}", exn.InputParameters);
                throw;
            }
            finally
            {
                // cleanup
                try { File.Delete(inputFilename); } catch { }
                try { File.Delete(outputFilename); } catch { }
                if (watermarkFilename != null)
                {
                    try { File.Delete(watermarkFilename); } catch { }
                }
            }
        }

        public async Task Thumbnail(IStreamProducer producer, IImageDestination consumer, ResizeOptions options, CancellationToken cancellationToken)
        {
            var inputFilename = Path.GetTempFileName();
            var thumbnailFilename = Path.ChangeExtension(inputFilename, "png");
            try
            {
                var stopwatch = new Stopwatch();
                _logger.LogDebug("Start reading input into local buffer [Path = {0}].", inputFilename);
                stopwatch.Restart();
                using (var stream = File.Create(inputFilename, 64 * 1024, FileOptions.Asynchronous))
                {
                    await producer.ProduceAsync(stream, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
                stopwatch.Stop();
                _logger.LogDebug("Reading input into local buffer took {0}ms [Path = {1}].", stopwatch.ElapsedMilliseconds, inputFilename);
                var mediaInfo = await GetMediaInfo(inputFilename).ConfigureAwait(false);

                if (!mediaInfo.VideoStreams.TryGetFirst(out var videoStream))
                {
                    throw new NoVideoStreamException();
                }
                var captureTime = videoStream.Duration / 2;
                var rotation = 0;
                try
                {
                    var rotStr = await Probe.New()
                        .Start($"-v error -select_streams v:0 -show_entries stream_tags=rotate -of csv=p=0 {inputFilename}");
                    int.TryParse(rotStr?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rotation);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to extract rotation.");
                }
                videoStream
                    .SetCodec(VideoCodec.png)
                    .SetOutputFramesCount(1)
                    .SetSeek(captureTime);

                if (rotation == 90 || rotation == 270)
                {
                    videoStream = videoStream.SetSize(videoStream.Height, videoStream.Width);
                }
                var conversion = FFmpeg.Conversions.New()
                    .AddStream(videoStream)
                    .SetOutput(thumbnailFilename)
                    .SetOverwriteOutput(true)
                    .UseMultiThread(true)
                    .SetPreset(ConversionPreset.Slow);
                _logger.LogDebug("Start thumbnail conversion [Source = {0}, Target = {1}].", inputFilename, thumbnailFilename);
                stopwatch.Restart();
                var result = await conversion.Start(cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                _logger.LogInformation("Thumbnail conversion took {0}ms [Arguments: {1}].", stopwatch.ElapsedMilliseconds, result.Arguments);
                await using var thumbnailProducer = StreamProducer.FromStream(new FileStream(thumbnailFilename, FileMode.Open, FileAccess.Read, FileShare.None, 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan));
                _logger.LogDebug("Start thumbnail resizing [Source = {0}, Options = {1}].", thumbnailFilename, options);
                stopwatch.Restart();
                await _imageResizer.ResizeAsync(thumbnailProducer, consumer, options, cancellationToken);
                stopwatch.Stop();
                _logger.LogDebug("Thumbnail resizing took {0}ms [Source = {1}, Options = {2}].", stopwatch.ElapsedMilliseconds, thumbnailFilename, options);
            }
            finally
            {
                // cleanup
                try { File.Delete(inputFilename); } catch { }
                try { File.Delete(thumbnailFilename); } catch { }
            }
        }

        public async ValueTask CreateThumbnailAsync(IVideoSource source, IImageDestination destination, ResizeOptions options, CancellationToken cancellationToken)
        {
            /*
            await using var producer = new GCSProducer(_storageClient, source.Host, source.LocalPath.Trim('/'));
            var consumerFactory = new GoogleCloudStorageDestination(
                uri: destination,
                credential: default,
                cacheControl: "public, max-age=31536000",
                isPublic: true,
                httpClientFactory: _httpClientFactory,
                logger: default
            );
            */
            await using var producer = source.CreateProducer();
            //await using var consumer = destination.CreateConsumer(new ContentInfo());
            await Thumbnail(producer, destination, options, cancellationToken);
        }

        public async ValueTask ResizeAsync(IVideoSource source, IVideoDestination destination, VideoOptions options, CancellationToken cancellationToken)
        {
            //await using var producer = new GCSProducer(_storageClient, source.Host, source.LocalPath.Trim('/'));
            // await using var consumer = new GCSConsumer(
            //     _storageClient,
            //     destination.Host,
            //     destination.LocalPath.Trim('/'),
            //     "video/mp4",
            //     "public, max-age=31536000",
            //     PredefinedObjectAcl.PublicRead
            // );
            await using var producer = source.CreateProducer();
            await using var consumer = destination.CreateConsumer(new ContentInfo("video/mp4"));
            await ResizeAsync(producer, consumer, options, cancellationToken);
        }


    }
}

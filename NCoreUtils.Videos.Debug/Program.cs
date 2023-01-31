using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.FFMpeg;
using NCoreUtils.Resources;

namespace NCoreUtils.Videos;

internal class Program
{
    private static void Main(string[] args)
    {
        var videoSettings = args.Length == 0
            ? "x264:preset=veryslow"// "x264:b=1536000:preset=veryslow"
            : args[0];
        AVLogging.LogLevel = AVLogLevel.AV_LOG_DEBUG;
        using var serviceProvider = new ServiceCollection()
            .AddLogging(b => b
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Debug)
                .AddFilter("NCoreUtils.FFMpeg", LogLevel.Warning)
                .AddFilter("FFMpeg", LogLevel.Warning)
                .AddSimpleConsole(o => o.SingleLine = true)
            )
            .AddFFMpegVideoResizer()
            .BuildServiceProvider(true);
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        AVLogging.SetLogger(loggerFactory);
        var resizer = serviceProvider.GetRequiredService<IVideoResizer>();
        /*
        resizer.ResizeAsync(
            new FileSystemResource("/home/artyom/Letöltések/sample-1.5m.mp4", default),
            new FileSystemResource("/tmp/out.mp4", default),
            new ResizeOptions(
                audioType: "none", //default,
                videoType: X264Settings.Parse(videoSettings, default),
                width: 150,
                height: 100,
                resizeMode: "inbox",
                quality: 85,
                optimize: true,
                weightX: 0,
                weightY: 0
            )
        ).AsTask().GetAwaiter().GetResult();
        */
        resizer.CreateThumbnailAsync(
            new FileSystemResource("/home/artyom/Letöltések/sample-1.5m.mp4", default),
            new FileSystemResource("/tmp/out.jpg", default),
            new ResizeOptions(),
            default
        ).GetAwaiter().GetResult();
    }
}
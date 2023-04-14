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
                .AddFilter("NCoreUtils.FFMpeg", LogLevel.Debug)
                .AddFilter("FFMpeg", LogLevel.Debug)
                .AddSimpleConsole(o => o.SingleLine = true)
            )
            .AddGoogleCloudStorageUtils()
            .AddFFMpegVideoResizer()
            .BuildServiceProvider(true);
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        AVLogging.SetLogger(loggerFactory);
        var resizer = serviceProvider.GetRequiredService<IVideoResizer>();
        var gutils = serviceProvider.GetRequiredService<GoogleCloudStorageUtils>();
        var source = new FileSystemResource("/home/artyom/Letöltések/sample-video.mp4", default);
        // var source = new GoogleCloudStorageResource(
        //     gutils,
        //     "skapeio",
        //     "sample-1.5m.mp4"
        // );
        var destination = new FileSystemResource("/tmp/out.jpg", default);
        // var destination = new GoogleCloudStorageResource(
        //     gutils,
        //     "skapeio",
        //     "test/output.mp4",
        //     "video/mp4",
        //     cacheControl: default,
        //     isPublic: false
        // );
        // resizer.ResizeAsync(
        //     source,
        //     destination,
        //     new ResizeOptions(
        //         audioType: "none",
        //         videoType: new X264Settings(default, default, "ultrafast"),
        //         resizeMode: "exact",
        //         width: 420
        //     )
        // ).AsTask().GetAwaiter().GetResult();
        resizer.CreateThumbnailAsync(
            source,
            destination,
            new ResizeOptions(),
            default
        ).GetAwaiter().GetResult();
    }
}
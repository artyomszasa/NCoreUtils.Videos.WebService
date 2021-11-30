using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.Videos.WebService.Client.Debug;

public class Program
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection()
            //.AddVideoResizerClient(new EndpointConfiguration() { Endpoint = "http://localhost:5000" })
            .BuildServiceProvider();

        using var sp = services.CreateScope();
        var resizer = sp.ServiceProvider.GetRequiredService<IVideoResizer>();
        var source = new GoogleCloudStorageSource(new Uri("gs://artyom-source/1280x720_120.mp4"));
        var dest = new GoogleCloudStorageDestination(new Uri("gs://artyom-target/xxx.mp4"));

        resizer.ResizeAsync(source, dest, new VideoOptions(videoType: "mp4", quality: 90, watermark: default, width: 640, height: 480), default).GetAwaiter().GetResult();



    }
}
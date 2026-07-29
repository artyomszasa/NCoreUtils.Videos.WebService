
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils;
using NCoreUtils.Videos;

var jobArguments = OptionsParser.ParseArguments(args);

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

var configuration = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
    .Build();

await using var services = new ServiceCollection()
    // logging
    .AddJobLogging(configuration)
    // http
    .AddHttpClient()
    // video resizer options
    .AddSingleton(configuration.GetSection("Videos").GetVideoResizerOptions())
    // ffmpeg
    .AddFFMpegVideoResizer()
    // source/destination handlers
    .AddResourceFactories()
#if EnableGoogleCloudStorage
#if EnableGoogleMetadataServer
    .AddGoogleCloudMetadataServer()
#else
    .AddGoogleCloudServiceAccount()
#endif
#endif
    // build DI container
    .BuildServiceProvider();

var resourceFactory = services.GetRequiredService<IResourceFactory>();
var resizer = services.GetRequiredService<IVideoResizer>();

var (source, destination) = ResolveSourceAndDestination(resourceFactory, jobArguments);
await resizer.ResizeAsync(source, destination, jobArguments.Options, cancellation.Token).ConfigureAwait(false);

[DoesNotReturn]
static void NotSupportedUri(Uri? uri)
    => throw new VideoException("unsupported_uri", $"Either invalid or unsupported uri: {uri}.");

static (IReadableResource Source, IWritableResource Destination) ResolveSourceAndDestination(
    IResourceFactory resourceFactory,
    JobArguments sd)
{
    if (!resourceFactory.TryCreateReadable(sd.Source, out var source))
    {
        NotSupportedUri(sd.Source);
    }
    if (!resourceFactory.TryCreateWritable(sd.Destination, out var destination))
    {
        NotSupportedUri(sd.Destination);
    }
    return (source, destination);
}
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos;

public class CoreFunctions : Generic.CoreFunctions
{
    public static Task InvokeCapabilities(HttpResponse response, CancellationToken cancellationToken)
        => InvokeCapabilities(new AspNetCoreHttpResponseWrapper(response), cancellationToken);

    public static Task InvokeResize(
        HttpRequest request,
        IResourceFactory resourceFactory,
        IVideoResizer resizer,
        CancellationToken cancellationToken)
        => InvokeResize(
            new AspNetCoreHttpRequestWrapper(request),
            resourceFactory,
            resizer,
            cancellationToken
        );

    public static ValueTask<VideoInfo> InvokeAnalyze(
        HttpRequest request,
        IResourceFactory resourceFactory,
        IVideoAnalyzer analyzer,
        CancellationToken cancellationToken)
        => InvokeAnalyze(
            new AspNetCoreHttpRequestWrapper(request),
            resourceFactory,
            analyzer,
            cancellationToken
        );


    public static Task InvokeAnalyze(
        HttpRequest request,
        HttpResponse response,
        IResourceFactory resourceFactory,
        IVideoAnalyzer analyzer,
        CancellationToken cancellationToken)
        => InvokeAnalyze(
            new AspNetCoreHttpRequestWrapper(request),
            new AspNetCoreHttpResponseWrapper(response),
            resourceFactory,
            analyzer,
            cancellationToken
        );

    public static Task InvokeThumbnail(
        HttpRequest request,
        IResourceFactory resourceFactory,
        IVideoResizer resizer,
        CancellationToken cancellationToken)
        => InvokeThumbnail(
            new AspNetCoreHttpRequestWrapper(request),
            resourceFactory,
            resizer,
            cancellationToken
        );
}
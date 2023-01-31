using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos;

public partial class VideosMiddleware
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool Eqi(string a, string b)
        => StringComparer.OrdinalIgnoreCase.Equals(a, b);

    private readonly RequestDelegate _next;

    private readonly PathString _resizeEndpoint;

    private readonly PathString _capabilitiesEndpoint;

    private readonly PathString _infoEndpoint;

    private readonly PathString _thumbnailEndpoint;

    public VideosMiddleware(RequestDelegate next)
    {
        var prefix = Environment.GetEnvironmentVariable("ENDPOINT_PREFIX") switch
        {
            null => "/",
            "" => "/",
            var pre => "/" + pre.Trim('/')
        };
        _resizeEndpoint = prefix;
        _capabilitiesEndpoint = prefix + Routes.Capabilities;
        _infoEndpoint = prefix + Routes.Info;
        _thumbnailEndpoint = prefix + Routes.Thumbnail;
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        // var (s, e) = GetSegment(request.Path.Value);
        // string path = request.Path.Value.Substring(s, e);
        if (request.Path == _resizeEndpoint && Eqi(request.Method, "POST"))
        {
            var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
            var resizer = context.RequestServices.GetRequiredService<IVideoResizer>();
            return CoreFunctions.InvokeResize(request, resourceFactory, resizer, context.RequestAborted);
        }
        if (request.Path == _capabilitiesEndpoint && Eqi(request.Method, "GET"))
        {
            return CoreFunctions.InvokeCapabilities(context.Response, context.RequestAborted);
        }
        if (request.Path == _infoEndpoint && Eqi(request.Method, "POST"))
        {
            var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
            var analyzer = context.RequestServices.GetRequiredService<IVideoAnalyzer>();
            return CoreFunctions.InvokeAnalyze(request, context.Response, resourceFactory, analyzer, context.RequestAborted);
        }
        if (request.Path == _thumbnailEndpoint && Eqi(request.Method, "POST"))
        {
            var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
            var resizer = context.RequestServices.GetRequiredService<IVideoResizer>();
            return CoreFunctions.InvokeThumbnail(request, resourceFactory, resizer, context.RequestAborted);
        }
        return _next(context);
    }
}
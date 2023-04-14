using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos.Function;

public class VideoFunctions
{
    private ILogger Logger { get; }

    private IResourceFactory ResourceFactory { get; }

    private IVideoResizer VideoResizer { get; }

    private IVideoAnalyzer VideoAnalyzer { get; }

    public VideoFunctions(ILogger<VideoFunctions> logger,  IResourceFactory resourceFactory, IVideoResizer videoResizer, IVideoAnalyzer videoAnalyzer)
    {
        Logger = logger;
        ResourceFactory = resourceFactory;
        VideoResizer = videoResizer;
        VideoAnalyzer = videoAnalyzer;
    }

    private static async Task<HttpResponseData> Error(HttpRequestData request, Exception exn, CancellationToken cancellationToken)
    {
        var data = Generic.ExceptionHelper.GetErrorData(exn);
        var response = request.CreateResponse(System.Net.HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await ErrorSerialization.SerializeVideoErrorDataAsync(response.Body, data, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private static HttpResponseData Ok(HttpRequestData request)
        => request.CreateResponse(System.Net.HttpStatusCode.OK);

    [Function("Resize")]
    public async Task<HttpResponseData> RunResize([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "video")] HttpRequestData request)
    {
        try
        {
            await CoreFunctions
                .InvokeResize(request, ResourceFactory, VideoResizer, request.FunctionContext.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            Logger.LogInformation(exn, "Failed to process request: {Exception}.", exn);
            return await Error(request, exn, request.FunctionContext.CancellationToken);
        }
        return Ok(request);
    }

    [Function("Analyze")]
    public async Task<HttpResponseData> RunAnalyze([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "video/" + Routes.Info)] HttpRequestData request)
    {
        try
        {
            return await CoreFunctions
                .InvokeAnalyze(request, ResourceFactory, VideoAnalyzer, request.FunctionContext.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            Logger.LogInformation(exn, "Failed to process request: {Exception}.", exn);
            return await Error(request, exn, request.FunctionContext.CancellationToken);
        }
    }

    [Function("Capabilities")]
    public static HttpResponseData RunCapabilities([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "video/" + Routes.Capabilities)] HttpRequestData request)
    {
        var response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        CoreFunctions.InvokeCapabilities(response, request.FunctionContext.CancellationToken);
        return response;
    }

    [Function("Thumbnail")]
    public async Task<HttpResponseData> RunThumbnail([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "video/" + Routes.Thumbnail)] HttpRequestData request)
    {
        try
        {
            var contentType = request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;
            Logger.LogInformation("Starting thumbnail request [Length = {ContentLength}, Type = {ContentType}].", request.Body.Length, contentType);
            await CoreFunctions
                .InvokeThumbnail(request, ResourceFactory, VideoResizer, request.FunctionContext.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            Logger.LogInformation(exn, "Failed to process request: {Exception}.", exn);
            return await Error(request, exn, request.FunctionContext.CancellationToken);
        }
        return Ok(request);
    }
}
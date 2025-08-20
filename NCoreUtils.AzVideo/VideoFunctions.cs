using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos.Function;

internal sealed class FakeRequest(string body, IReadOnlyDictionary<string, StringValues> query) : Generic.IHttpRequest
{
    public string? ContentType => "application/json";

    public Stream Body { get; } = new MemoryStream(Convert.FromBase64String(body), writable: false);

    public bool TryGetQueryParameter(string key, out StringValues values)
        => query.TryGetValue(key, out values);
}

public sealed class SerializedRequest(string body, string query)
{
    public string Body { get; } = body;

    public string Query { get; } = query;
}

[JsonSerializable(typeof(SerializedRequest))]
internal partial class SerializedRequestSerializerContext : JsonSerializerContext { }

public class VideoFunctions(ILogger<VideoFunctions> logger, IResourceFactory resourceFactory, IVideoResizer videoResizer, IVideoAnalyzer videoAnalyzer)
{
    private static long? GetLength(Stream source)
    {
        try
        {
            return source.Length;
        }
        catch
        {
            return default;
        }
    }

    private ILogger Logger { get; } = logger;

    private IResourceFactory ResourceFactory { get; } = resourceFactory;

    private IVideoResizer VideoResizer { get; } = videoResizer;

    private IVideoAnalyzer VideoAnalyzer { get; } = videoAnalyzer;

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
            var contentType = request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;
            Logger.LogInformation("Starting resize request [Length = {ContentLength}, Type = {ContentType}].", GetLength(request.Body), contentType);
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
            var contentType = request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;
            Logger.LogInformation("Starting analyze request [Length = {ContentLength}, Type = {ContentType}].", GetLength(request.Body), contentType);
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
            Logger.LogInformation("Starting thumbnail request [Length = {ContentLength}, Type = {ContentType}].", GetLength(request.Body), contentType);
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

    [Function(nameof(RunScheduledResize))]
    public async Task RunScheduledResize([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<string>() ?? throw new InvalidOperationException("Input is null.");
        var logger = context.CreateReplaySafeLogger<VideoFunctions>();
        logger.LogInformation("Starting scheduled resize request [Length = {ContentLength}].", input.Length);
        await context.CallActivityAsync(nameof(DoRunScheduledResize), input);
    }

    [Function(nameof(DoRunScheduledResize))]
    public async Task DoRunScheduledResize([ActivityTrigger] string serializedRequest, FunctionContext functionContext)
    {
        var requestData = JsonSerializer.Deserialize(serializedRequest, SerializedRequestSerializerContext.Default.SerializedRequest)
            ?? throw new InvalidOperationException("Could not deserialize request.");
        var query = QueryHelpers.ParseQuery(requestData.Query);
        var request = new FakeRequest(requestData.Body, query);
        await CoreFunctions
            .InvokeResize(request, ResourceFactory, VideoResizer, functionContext.CancellationToken)
            .ConfigureAwait(false);
    }

    [Function("ScheduleResize")]
    public async Task<HttpResponseData> RunScheduleResize(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "video/schedule")] HttpRequestData request,
        [DurableClient] DurableTaskClient client)
    {
        var query = request.Url.Query;
        string body;
        {
            var buffer = new byte[4 * 1024];
            await using var bufferStream = new MemoryStream(buffer, writable: true);
            await request.Body.CopyToAsync(bufferStream, request.FunctionContext.CancellationToken).ConfigureAwait(false);
            body = Convert.ToBase64String(buffer.AsSpan()[..(int)bufferStream.Position]);
        }
        // var retryOpts = TaskRetryOptions.FromRetryHandler(context =>
        // {
        //     return context.LastAttemptNumber < 3;
        // });
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName: nameof(RunScheduledResize),
            input: JsonSerializer.Serialize(new SerializedRequest(body, query), SerializedRequestSerializerContext.Default.SerializedRequest),
            // options: new StartOrchestrationOptions(retryOpts)
            cancellation: request.FunctionContext.CancellationToken
        );

        request.FunctionContext.GetLogger<VideoFunctions>()
            .LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(request, instanceId);
    }
}
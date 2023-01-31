using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos;

public class ErrorMiddleware
{
    private static VideoErrorData GetErrorData(VideoException exn) => exn switch
    {
        UnsupportedResizeModeException e => new UnsupportedResizeModeData(e.ErrorCode, e.Message, e.ResizeMode, e.Width, e.Height),
        UnsupportedVideoTypeException e => new UnsupportedVideoTypeData(e.ErrorCode, e.Message, e.VideoType),
        InternalVideoException e => new InternalVideoErrorData(e.ErrorCode, e.Message, e.InternalCode),
        VideoException e => new VideoErrorData(e.ErrorCode, e.Message)
    };

    private readonly RequestDelegate _next;

    private readonly ILogger _logger;

    public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            var response = context.Response;
            if (response.HasStarted)
            {
                throw;
            }
            _logger.LogWarning(exn, "Failes to process request.");
            VideoErrorData data = exn is VideoException e ? GetErrorData(e) : new VideoErrorData(ErrorCodes.GenericError, exn.Message);
            response.StatusCode = 400;
            response.ContentType = "application/json; charset=utf-8";
            await ErrorSerialization.SerializeVideoErrorDataAsync(response.Body, data, context.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        }
    }
}
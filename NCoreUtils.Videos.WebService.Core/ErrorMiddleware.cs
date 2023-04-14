using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos;

public class ErrorMiddleware
{
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
            VideoErrorData data = Generic.ExceptionHelper.GetErrorData(exn);
            response.StatusCode = 400;
            response.ContentType = "application/json; charset=utf-8";
            await ErrorSerialization.SerializeVideoErrorDataAsync(response.Body, data, context.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        }
    }
}
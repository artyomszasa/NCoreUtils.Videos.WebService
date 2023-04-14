using System.Globalization;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Primitives;

namespace NCoreUtils.Videos.Function;

internal sealed class HttpRequestAdapter : Generic.IHttpRequest
{
    private HttpRequestData Request { get; }

    public string? ContentType
        => Request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;

    public Stream Body => Request.Body;

    public HttpRequestAdapter(HttpRequestData request)
        => Request = request ?? throw new ArgumentNullException(nameof(request));

    public bool TryGetQueryParameter(string key, out StringValues values)
    {
        if (Request.Query.AllKeys.Contains(key))
        {
            var value = Request.Query[key];
            if (!string.IsNullOrEmpty(value))
            {
                values = value;
                return true;
            }
        }
        values = default;
        return false;
    }
}

internal sealed class HttpResponseAdapter : Generic.IHttpResponse
{
    internal HttpResponseData Response { get; }

    public string ContentType
    {
        set
        {
            Response.Headers.Remove("Content-Type");
            Response.Headers.Add("Content-Type", value);
        }
    }

    public long? ContentLength
    {
        set
        {
            Response.Headers.Remove("Content-Length");
            if (value is long contentLength)
            {
                Response.Headers.Add("Content-Length", contentLength.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public Stream Body => Response.Body;

    public HttpResponseAdapter(HttpResponseData response)
        => Response = response ?? throw new ArgumentNullException(nameof(response));
}

internal class CoreFunctions : Generic.CoreFunctions
{
    public static Task InvokeCapabilities(HttpResponseData response, CancellationToken cancellationToken)
        => InvokeCapabilities(new HttpResponseAdapter(response), cancellationToken);

    public static Task InvokeResize(
        HttpRequestData request,
        IResourceFactory resourceFactory,
        IVideoResizer resizer,
        CancellationToken cancellationToken)
        => InvokeResize(
            new HttpRequestAdapter(request),
            resourceFactory,
            resizer,
            cancellationToken
        );

    // public static ValueTask<VideoInfo> InvokeAnalyze(
    //     HttpRequestData request,
    //     IResourceFactory resourceFactory,
    //     IVideoAnalyzer analyzer,
    //     CancellationToken cancellationToken)
    //     => InvokeAnalyze(
    //         new HttpRequestAdapter(request),
    //         resourceFactory,
    //         analyzer,
    //         cancellationToken
    //     );


    public static async Task<HttpResponseData> InvokeAnalyze(
        HttpRequestData request,
        IResourceFactory resourceFactory,
        IVideoAnalyzer analyzer,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseAdapter(request.CreateResponse());
        await InvokeAnalyze(
            new HttpRequestAdapter(request),
            response,
            resourceFactory,
            analyzer,
            cancellationToken
        ).ConfigureAwait(false);
        return response.Response;
    }

    public static Task InvokeThumbnail(
        HttpRequestData request,
        IResourceFactory resourceFactory,
        IVideoResizer resizer,
        CancellationToken cancellationToken)
        => InvokeThumbnail(
            new HttpRequestAdapter(request),
            resourceFactory,
            resizer,
            cancellationToken
        );
}
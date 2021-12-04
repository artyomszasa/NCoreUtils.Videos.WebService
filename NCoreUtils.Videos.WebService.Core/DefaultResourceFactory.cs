using System;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.Videos
{
    public class DefaultResourceFactory : IResourceFactory
    {
        readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultResourceFactory(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            if (uri is null)
            {
                var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access current HTTP context.");
                return new HttpResponseDestination(context.Response);
            }
            return next();
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            if (uri is null)
            {
                var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access current HTTP context.");
                return new HttpRequestSource(context.Request);
            }
            return next();
        }
    }
}
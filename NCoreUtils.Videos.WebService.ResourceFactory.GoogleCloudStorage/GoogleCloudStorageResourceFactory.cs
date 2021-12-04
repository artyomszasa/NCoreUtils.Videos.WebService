using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Extensions.Logging;
using NCoreUtils.Videos.GoogleCloudStorage;

namespace NCoreUtils.Videos
{
    public class GoogleCloudStorageResourceFactory : IResourceFactory
    {
        static readonly HashSet<string> _truthy = new(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "t",
            "on",
            "1"
        };

        static Uri StripQuery(Uri source)
            => new UriBuilder(source) { Query = string.Empty }.Uri;

        readonly ILoggerFactory _loggerFactory;

        readonly GoogleCloudStorageUtils? _gcsUtils;

        public GoogleCloudStorageResourceFactory(ILoggerFactory loggerFactory, GoogleCloudStorageUtils? gcsUtils = default)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _gcsUtils = gcsUtils;
        }

        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            if (null != uri && uri.Scheme == "gs")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var accessToken = q.Get(UriParameters.AccessToken) ?? throw new InvalidOperationException("No access token provided in GCS destination uri.");
                var cacheControl = q.Get(UriParameters.CacheControl);
                var contentType = q.Get(UriParameters.ContentType);
                var rawPublic = q.Get(UriParameters.Public);
                var isPublic = !string.IsNullOrEmpty(rawPublic) && _truthy.Contains(rawPublic);
                return new GoogleCloudStorageDestination(
                    uri: StripQuery(uri),
                    credential: GoogleStorageCredential.ViaAccessToken(accessToken),
                    contentType: contentType,
                    cacheControl: cacheControl,
                    isPublic: isPublic,
                    utils: _gcsUtils,
                    logger: _loggerFactory.CreateLogger<GoogleCloudStorageDestination>()
                );
            }
            return next();
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            if (null != uri && uri.Scheme == "gs")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var accessToken = q.Get(UriParameters.AccessToken) ?? throw new InvalidOperationException("No access token provided in GCS source uri.");
                return new GoogleCloudStorageSource(
                    uri: StripQuery(uri),
                    credential: GoogleStorageCredential.ViaAccessToken(accessToken),
                    utils: _gcsUtils,
                    logger: _loggerFactory.CreateLogger<GoogleCloudStorageSource>()
                );
            }
            return next();
        }
    }
}
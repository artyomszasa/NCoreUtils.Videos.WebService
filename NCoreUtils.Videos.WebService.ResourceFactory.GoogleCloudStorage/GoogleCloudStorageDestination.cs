using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.GoogleCloudStorage;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public class GoogleCloudStorageDestination : NCoreUtils.Images.GoogleCloudStorageDestination, IVideoDestination, ISerializableVideoResource
    {
        public GoogleCloudStorageDestination(Uri uri, GoogleStorageCredential credential = default, string? contentType = null, string? cacheControl = null, bool isPublic = false, IHttpClientFactory? httpClientFactory = null, ILogger<Images.GoogleCloudStorageDestination>? logger = null) : base(uri, credential, contentType, cacheControl, isPublic, httpClientFactory, logger)
        {
        }

        public IStreamConsumer CreateConsumer()
            => this.CreateConsumer(new Images.ContentInfo());
    }
}
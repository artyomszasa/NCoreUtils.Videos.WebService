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
    public class GoogleCloudStorageSource : NCoreUtils.Images.GoogleCloudStorageSource, IVideoSource, ISerializableVideoResource
    {
        public GoogleCloudStorageSource(Uri uri, GoogleStorageCredential credential = default, IHttpClientFactory? httpClientFactory = null, ILogger<Images.GoogleCloudStorageSource>? logger = null) : base(uri, credential, httpClientFactory, logger)
        {
        }
    }
}
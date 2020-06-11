using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using NCoreUtils.IO;
using GCSObject = Google.Apis.Storage.v1.Data.Object;

namespace NCoreUtils.Videos.WebService
{
    internal class GCSConsumer : IStreamConsumer
    {
        public StorageClient StorageClient { get; }

        public string BucketName { get; }

        public string ObjectName { get; }

        public string ContentType { get; }

        public string CacheControl { get; }

        public PredefinedObjectAcl PredefinedAcl { get; }

        public GCSConsumer(
            StorageClient storageClient,
            string bucketName,
            string objectName,
            string contentType,
            string cacheControl,
            PredefinedObjectAcl predefinedAcl)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException($"'{nameof(bucketName)}' cannot be null or empty", nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentException($"'{nameof(objectName)}' cannot be null or empty", nameof(objectName));
            }
            StorageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
            BucketName = bucketName;
            ObjectName = objectName;
            ContentType = contentType;
            CacheControl = cacheControl;
            PredefinedAcl = predefinedAcl;
        }

        public ValueTask ConsumeAsync(Stream input, CancellationToken cancellationToken = default)
        {
            ulong? inputLength;
            try
            {
                inputLength = Convert.ToUInt64(input.Length);
            }
            catch
            {
                inputLength = default;
            }
            var gcsObject = new GCSObject
            {
                Bucket = BucketName,
                Name = ObjectName,
                CacheControl = CacheControl,
                ContentType = ContentType,
                Size = inputLength
            };
            return new ValueTask(StorageClient.UploadObjectAsync(
                destination: gcsObject,
                source: input,
                options: new UploadObjectOptions { PredefinedAcl = PredefinedAcl },
                cancellationToken: cancellationToken
            ));
        }

        public ValueTask DisposeAsync() => default;
    }
}
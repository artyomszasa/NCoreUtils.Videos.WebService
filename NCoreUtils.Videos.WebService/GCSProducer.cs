using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using NCoreUtils.IO;

namespace NCoreUtils.Videos.WebService
{
    internal class GCSProducer : IStreamProducer
    {
        public StorageClient StorageClient { get; }

        public string BucketName { get; }

        public string ObjectName { get; }

        public GCSProducer(StorageClient storageClient, string bucketName, string objectName)
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
        }

        public ValueTask DisposeAsync() => default;

        public ValueTask ProduceAsync(Stream output, CancellationToken cancellationToken = default)
            => new ValueTask(StorageClient.DownloadObjectAsync(BucketName, ObjectName, output, cancellationToken: cancellationToken));
    }
}
using System;
using System.IO;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public class FileSystemDestination : IVideoDestination, ISerializableVideoResource
    {
        public const int DefaultBufferSize = 32 * 1024;

        public string AbsolutePath { get; }

        public int? BufferSize { get; }

        public Uri Uri => new($"file://{AbsolutePath}", UriKind.Absolute);

        public FileSystemDestination(string absolutePath, int? bufferSize = default)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                throw new ArgumentException($"'{nameof(absolutePath)}' cannot be null or whitespace.", nameof(absolutePath));
            }
            AbsolutePath = absolutePath;
            BufferSize = bufferSize;
        }

        public IStreamConsumer CreateConsumer(ContentInfo contentInfo)
            => StreamConsumer.ToStream(new FileStream(
                AbsolutePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.ReadWrite,
                BufferSize ?? DefaultBufferSize,
                FileOptions.WriteThrough | FileOptions.Asynchronous
            ), BufferSize ?? DefaultBufferSize);
    }
}
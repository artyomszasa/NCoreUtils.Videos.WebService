using System;
using System.IO;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public class FileSystemSource : IVideoSource, ISerializableVideoResource
    {
        public const int DefaultBufferSize = 32 * 1024;

        public string AbsolutePath { get; }

        public int? BufferSize { get; }

        public Uri Uri => new($"file://{AbsolutePath}", UriKind.Absolute);

        public bool Reusable => true;

        public FileSystemSource(string absolutePath, int? bufferSize = default)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                throw new ArgumentException($"'{nameof(absolutePath)}' cannot be null or whitespace.", nameof(absolutePath));
            }
            AbsolutePath = absolutePath;
            BufferSize = bufferSize;
        }

        public IStreamProducer CreateProducer()
            => StreamProducer.FromStream(new FileStream(
                AbsolutePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize ?? DefaultBufferSize,
                true
            ), BufferSize ?? DefaultBufferSize);
    }
}
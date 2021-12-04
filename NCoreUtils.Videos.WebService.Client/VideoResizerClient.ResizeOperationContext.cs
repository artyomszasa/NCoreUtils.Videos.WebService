using System.Net.Http.Headers;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public partial class VideoResizerClient
    {
        protected sealed class ResizeOperationContext
        {
            static readonly MediaTypeHeaderValue _binary = MediaTypeHeaderValue.Parse("application/octet-stream");

            static readonly MediaTypeHeaderValue _json = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            public static ResizeOperationContext Inline(IStreamProducer producer, IVideoDestination destination)
                => new ResizeOperationContext(_binary, producer, destination);

            public static ResizeOperationContext Json(IStreamProducer producer, IVideoDestination? destination = default)
                => new ResizeOperationContext(_json, producer, destination);

            public MediaTypeHeaderValue ContentType { get; }

            public IStreamProducer Producer { get; }

            public IVideoDestination? Destination { get; }

            ResizeOperationContext(MediaTypeHeaderValue contentType, IStreamProducer producer, IVideoDestination? destination)
            {
                ContentType = contentType;
                Producer = producer;
                Destination = destination;
            }
        }
    }
}
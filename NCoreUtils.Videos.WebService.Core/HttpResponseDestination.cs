using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public class HttpResponseDestination : IVideoDestination
    {
        const int BufferSize = 16 * 1024;

        readonly HttpResponse _response;

        public HttpResponseDestination(HttpResponse response)
            => _response = response ?? throw new ArgumentNullException(nameof(response));

        public IStreamConsumer CreateConsumer(ContentInfo contentInfo)
        {
            if (!string.IsNullOrEmpty(contentInfo.Type))
            {
                _response.ContentType = contentInfo.Type;
            }
            if (contentInfo.Length.HasValue)
            {
                _response.ContentLength = contentInfo.Length;
            }
            return StreamConsumer.Create((input, cancellationToken) => new ValueTask(input.CopyToAsync(_response.Body, BufferSize, cancellationToken)));
        }
    }
}
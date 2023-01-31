using System;
using System.Collections.Generic;

namespace NCoreUtils.Videos.Internal;

public class ExactResizerFactory : IResizerFactory
{
    private sealed class ExactResizer : IResizer
    {
        readonly Size _size;

        public ExactResizer(Size size)
            => _size = size;

        public IAsyncEnumerable<VideoTransformation> PopulateTransformations(IVideo videoInfo)
            // FIMXE: do not alloc array
            => new AsyncVideoTransformationEnumerable(new VideoTransformation[]
            {
                VideoTransformation.Resize(_size)
            });
    }

    public static ExactResizerFactory Instance { get; } = new ExactResizerFactory();

    private ExactResizerFactory() { }

    public IResizer CreateResizer(IVideo video, ResizeOptions options)
    {
        Size size;
        if (options.Width.HasValue)
        {
            if (options.Height.HasValue)
            {
                size = new Size(options.Width.Value, options.Height.Value);
            }
            else
            {
                var videoSize = video.Size;
                size = new Size(
                    options.Width.Value,
                    (int)((double)videoSize.Height / videoSize.Width * options.Width.Value)
                );
            }
        }
        else
        {
            if (options.Height.HasValue)
            {
                var videoSize = video.Size;
                size = new Size(
                    (int)((double)videoSize.Width / videoSize.Height * options.Height.Value),
                    options.Height.Value
                );
            }
            else
            {
                throw new InvalidOperationException("Output video dimensions must be specified when using exact resizing.");
            }
        }
        return new ExactResizer(size);
    }
}
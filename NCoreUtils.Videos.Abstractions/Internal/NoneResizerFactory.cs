using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal;

public class NoneResizerFactory : IResizerFactory
{
    private sealed class NoTransformationsEnumerator : IAsyncEnumerator<VideoTransformation>
    {
        public static NoTransformationsEnumerator Singleton { get; } = new();

        public VideoTransformation Current => default;

        private NoTransformationsEnumerator() { /* noop */ }

        public ValueTask DisposeAsync()
            => default; // noop

        public ValueTask<bool> MoveNextAsync()
            => default; // false
    }

    public sealed class NoneResizer : IResizer, IAsyncEnumerable<VideoTransformation>
    {
        public static NoneResizer Instance { get; } = new NoneResizer();

        NoneResizer() { }

        public IAsyncEnumerable<VideoTransformation> PopulateTransformations(IVideo videoInfo)
            => this;

        IAsyncEnumerator<VideoTransformation> IAsyncEnumerable<VideoTransformation>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => NoTransformationsEnumerator.Singleton;
    }


    public IResizer CreateResizer(IVideo video, ResizeOptions options) => NoneResizer.Instance;
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal;

internal sealed class AsyncVideoTransformationEnumerator : IAsyncEnumerator<VideoTransformation>
{
    private IReadOnlyList<VideoTransformation> Source { get; }

    private CancellationToken CancellationToken { get; }

    private int Index { get; set; }

    public VideoTransformation Current => Source[Index];

    public AsyncVideoTransformationEnumerator(IReadOnlyList<VideoTransformation> source, CancellationToken cancellationToken)
    {
        Source = source;
        Index = -1;
        CancellationToken = cancellationToken;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        CancellationToken.ThrowIfCancellationRequested();
        if (Index >= Source.Count)
        {
            return default; // false
        }
        return new(++Index < Source.Count);
    }

    public ValueTask DisposeAsync()
        => default; // noop
}
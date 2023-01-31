using System;
using System.Collections.Generic;
using System.Threading;

namespace NCoreUtils.Videos.Internal;

public sealed class AsyncVideoTransformationEnumerable : IAsyncEnumerable<VideoTransformation>
{
    public IReadOnlyList<VideoTransformation> Source { get; }

    public AsyncVideoTransformationEnumerable(IReadOnlyList<VideoTransformation> source)
        => Source = source ?? throw new ArgumentNullException(nameof(source));

    public IAsyncEnumerator<VideoTransformation> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncVideoTransformationEnumerator(Source, cancellationToken);
}
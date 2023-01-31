using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal;

internal static class AsyncEnumerableExtensions
{
    private static async ValueTask<IReadOnlyList<VideoTransformation>> GenericToListAsync(
        IAsyncEnumerable<VideoTransformation> source,
        CancellationToken cancellationToken)
    {
        var list = new List<VideoTransformation>();
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(item);
        }
        return list;
    }

    private static async ValueTask<IReadOnlyList<VideoTransformation>> GenericToListAsync(
        IAsyncEnumerable<VideoTransformation> source)
    {
        var list = new List<VideoTransformation>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            list.Add(item);
        }
        return list;
    }

    public static ValueTask<IReadOnlyList<VideoTransformation>> ToListAsync(this IAsyncEnumerable<VideoTransformation> source, CancellationToken cancellationToken = default)
        => source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            AsyncVideoTransformationEnumerable listSource => new(listSource.Source),
            NoneResizerFactory.NoneResizer _ => new(Array.Empty<VideoTransformation>()),
            _ when cancellationToken.CanBeCanceled => GenericToListAsync(source, cancellationToken),
            _ => GenericToListAsync(source)
        };
}
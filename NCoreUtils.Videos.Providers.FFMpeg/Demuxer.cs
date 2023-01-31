using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class Demuxer : IConsumer<AVPacket>
{
    private int _isDisposed;

    private IReadOnlyDictionary<int, IConsumer<AVPacket>> Consumers { get; }

    public Demuxer(IReadOnlyDictionary<int, IConsumer<AVPacket>> consumers)
        => Consumers = consumers ?? throw new ArgumentNullException(nameof(consumers));

    public void Consume(AVPacket item)
    {
        if (Consumers.TryGetValue(item.StreamIndex, out var consumer))
        {
            consumer.Consume(item);
        }
    }

    public void Flush()
    {
        foreach (var (_, consumer) in Consumers)
        {
            consumer.Flush();
        }
    }

    public void Dispose()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            foreach (var (_, consumer) in Consumers)
            {
                consumer.Dispose();
            }
        }
    }
}
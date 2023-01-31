using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class Muxer : IConsumer<AVPacket>
{
    private int _isDisposed;

    private int _bindCount;

    private int _flushCount;

    private IConsumer<AVPacket> Consumer { get; }

    public Muxer(IConsumer<AVPacket> consumer)
        => Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));

    void IConsumer<AVPacket>.NotifyBound(object producer)
        => Interlocked.Increment(ref _bindCount);

    public void Consume(AVPacket item)
        => Consumer.Consume(item);

    public void Flush()
    {
        if (_bindCount == Interlocked.Increment(ref _flushCount))
        {
            Consumer.Flush();
        }
    }

    public void Dispose()
    {
        if (0 >= Interlocked.Decrement(ref _bindCount) && 0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            Consumer.Dispose();
        }
    }
}
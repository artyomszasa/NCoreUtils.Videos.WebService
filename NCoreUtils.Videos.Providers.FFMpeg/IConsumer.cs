using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Videos.FFMpeg;

internal interface IConsumer<T> : IDisposable
{
    void NotifyBound(object producer) { /* noop */ }

    void Consume(T item);

    void Flush();
}

internal abstract class ProcessorBase<TSource, TResult> : IConsumer<TSource>
{
    private int _isDisposed;

    protected IConsumer<TResult> Consumer { get; }

    protected ProcessorBase(IConsumer<TResult> consumer)
    {
        Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        Consumer.NotifyBound(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            Consumer.Dispose();
        }
    }

    protected abstract void ConsumeInput(TSource? item);

    protected abstract bool TryProduceOutput([MaybeNullWhen(false)] out TResult result);

    protected virtual void PushOutput(TResult result)
        => Consumer.Consume(result);

    public void Consume(TSource item)
    {
        ConsumeInput(item);
        while (TryProduceOutput(out var outItem))
        {
            PushOutput(outItem);
        }
    }

    public void Flush()
    {
        ConsumeInput(default);
        while (TryProduceOutput(out var outItem))
        {
            PushOutput(outItem);
        }
        Consumer.Flush();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
}
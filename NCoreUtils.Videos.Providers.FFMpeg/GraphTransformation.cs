using System.Diagnostics.CodeAnalysis;
using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class GraphTransformation : ProcessorBase<AVFrame, AVFrame>
{
    private int _isDisposed;

    private AVFilterGraph Graph { get; }

    private AVFilterContext InBuffer { get; }

    private AVFilterContext OutBuffer { get; }

    private AVFrame Frame { get; }

    public GraphTransformation(AVFilterGraph graph, AVFilterContext inBuffer, AVFilterContext outBuffer, IConsumer<AVFrame> consumer)
        : base(consumer)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        InBuffer = inBuffer ?? throw new ArgumentNullException(nameof(inBuffer));
        OutBuffer = outBuffer ?? throw new ArgumentNullException(nameof(outBuffer));
        Frame = AVFrame.CreateFrame();
    }

    protected override void ConsumeInput(AVFrame? item)
    {
        if (item is null)
        {
            InBuffer.BufferSrcAddFlushFrame();
        }
        else
        {
            InBuffer.BufferSrcAddFrame(item);
        }
    }

    protected override bool TryProduceOutput([MaybeNullWhen(false)] out AVFrame result)
    {
        if (OutBuffer.BufferSinkGetFrame(Frame))
        {
            result = Frame;
            return true;
        }
        result = default;
        return false;
    }

    protected override void PushOutput(AVFrame result)
    {
        base.PushOutput(result);
        result.Unref();
    }

    protected override void Dispose(bool disposing)
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            if (disposing)
            {
                Frame.Dispose();
                Graph.Dispose();
            }
        }
    }
}
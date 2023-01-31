using System.Diagnostics.CodeAnalysis;
using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class Decoder : ProcessorBase<AVPacket, AVFrame>
{
    private int _isDisposed;

    private AVCodecContext DecoderCtx { get; }

    private AVFrame Frame { get; }

    public Decoder(AVCodecContext decoderCtx, IConsumer<AVFrame> consumer)
        : base(consumer)
    {
        DecoderCtx = decoderCtx;
        Frame = AVFrame.CreateFrame();
    }

    protected override void ConsumeInput(AVPacket? item)
    {
        DecoderCtx.EnsureOpened();
        if (item is null)
        {
            DecoderCtx.SendFlushPacket();
        }
        else
        {
            DecoderCtx.SendPacket(item);
        }
    }

    protected override bool TryProduceOutput([MaybeNullWhen(false)] out AVFrame result)
    {
        if (DecoderCtx.TryReceiveFrame(Frame))
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
                DecoderCtx.Dispose();
            }
        }
    }

    public override string ToString()
        => $"Decoder[{DecoderCtx}]";
}
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

/// <summary>
/// Encodes frames and sets packet stream index.
/// </summary>
internal sealed class Encoder : ProcessorBase<AVFrame, AVPacket>
{
    private int _isDisposed;

    private AVCodecContext EncoderCtx { get; }

    private int TargetStreamIndex { get; }

    private AVPacket Packet { get; }

    public Encoder(
        AVCodecContext encoderCtx,
        int targetStreamIndex,
        IConsumer<AVPacket> consumer)
        : base(consumer)
    {
        EncoderCtx = encoderCtx ?? throw new ArgumentNullException(nameof(encoderCtx));
        TargetStreamIndex = targetStreamIndex;
        Packet = AVPacket.CreatePacket();
    }

    protected override void Dispose(bool disposing)
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            if (disposing)
            {
                Packet.Dispose();
                EncoderCtx.Dispose();
            }
        }
    }

    protected override void ConsumeInput(AVFrame? item)
    {
        EncoderCtx.EnsureOpened();
        if (item is null)
        {
            EncoderCtx.SendFlushFrame();
        }
        else
        {
            EncoderCtx.SendFrame(item);
        }
    }

    protected override bool TryProduceOutput([MaybeNullWhen(false)] out AVPacket result)
    {
        if (EncoderCtx.TryReceivePacket(Packet))
        {
            result = Packet;
            return true;
        }
        result = default;
        return false;
    }

    protected override void PushOutput(AVPacket result)
    {
        result.StreamIndex = TargetStreamIndex;
        base.PushOutput(result);
        result.Unref();
    }

    public override string ToString()
        => $"Encoder[{EncoderCtx}]";
}
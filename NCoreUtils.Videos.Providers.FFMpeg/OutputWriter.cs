using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class OutputWriter : IConsumer<AVPacket>
{
    private int _isDisposed;

    private int _started;

    private AVFormatOutputContext OutCtx { get; }

    // FIXME: use list
    private IReadOnlyDictionary<int, AVRational> SourceTimeBases { get; }

    public OutputWriter(AVFormatOutputContext outCtx, IReadOnlyDictionary<int, AVRational> sourceTimeBases)
    {
        OutCtx = outCtx ?? throw new ArgumentNullException(nameof(outCtx));
        SourceTimeBases = sourceTimeBases ?? throw new ArgumentNullException(nameof(sourceTimeBases));
    }

    public void Consume(AVPacket packet)
    {
        if (0 == _started && 0 == Interlocked.CompareExchange(ref _started, 1, 0))
        {
            if (OutCtx.IoContext?.CanSeek == true)
            {
                OutCtx.WriteHeader();
            }
            else
            {
                var options = new AVDictionary() { { "movflags", "+frag_keyframe+empty_moov" } };
                OutCtx.WriteHeader(ref options);
            }
        }
        var streamIndex = packet.StreamIndex;
        var sourceTimeBase = SourceTimeBases[streamIndex];
        var outStream = OutCtx.Streams[0];
        var destinationTimeBase = outStream.TimeBase;
        if (sourceTimeBase != destinationTimeBase)
        {
            packet.RescaleTimestamp(sourceTimeBase, destinationTimeBase);
        }
        packet.Position = -1;
        OutCtx.InterleavedWriteFrame(packet);
    }

    public void Flush()
    {
        OutCtx.InterleavedFlush();
        OutCtx.WriteTrailer();
        // OutCtx.Dump(0, "out");
    }

    public void Dispose()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            OutCtx.Dispose();
        }
    }
}
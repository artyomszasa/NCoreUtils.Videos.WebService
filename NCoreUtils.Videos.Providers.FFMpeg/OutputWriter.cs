using System.Runtime.InteropServices;
using NCoreUtils.FFMpeg;

namespace NCoreUtils.Videos.FFMpeg;

internal sealed class OutputWriter(AVFormatOutputContext outCtx, IReadOnlyDictionary<int, AVRational> sourceTimeBases) : IConsumer<AVPacket>
{
    // FIXME: move to FFMpeg
    private const long AV_NOPTS_VALUE = unchecked((long)0x8000000000000000);

    private int _isDisposed;

    private int _started;

    // private long _lastDts = AV_NOPTS_VALUE;

    private readonly Dictionary<int, long> _lastDtss = [];

    private AVFormatOutputContext OutCtx { get; } = outCtx ?? throw new ArgumentNullException(nameof(outCtx));

    // FIXME: use list
    /// <summary>
    /// SourceTimeBases indexed by OUT stream index
    /// </summary>
    private IReadOnlyDictionary<int, AVRational> SourceTimeBases { get; } = sourceTimeBases ?? throw new ArgumentNullException(nameof(sourceTimeBases));

    private ref long LastDts(int streamIndex)
    {
        ref var lastDts = ref CollectionsMarshal.GetValueRefOrAddDefault(_lastDtss, streamIndex, out var exists);
        if (!exists)
        {
            lastDts = AV_NOPTS_VALUE;
        }
        return ref lastDts;
    }

    public void Consume(AVPacket packet)
    {
        // NOTE: packet contains OUT stream index
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

        // NOTE: rescale timelines
        var streamIndex = packet.StreamIndex;
        var sourceTimeBase = SourceTimeBases[streamIndex];
        var outStream = OutCtx.Streams[streamIndex];
        var destinationTimeBase = outStream.TimeBase;
        if (sourceTimeBase != destinationTimeBase)
        {
            packet.RescaleTimestamp(sourceTimeBase, destinationTimeBase);
        }

        ref var lastDts = ref LastDts(outStream.Index);
        if (lastDts != AV_NOPTS_VALUE && packet.Dts < lastDts)
        {
            return; // skip frame
        }
        lastDts = packet.Dts;


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
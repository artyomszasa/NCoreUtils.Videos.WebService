using NCoreUtils.FFMpeg;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos.FFMpeg;

public sealed class Video : IVideo
{
    internal readonly struct GraphConfiguration
    {
        public AVFilterGraph Graph { get; }

        public AVFilterContext Src { get; }

        public AVFilterContext Sink { get; }

        public GraphConfiguration(AVFilterGraph graph, AVFilterContext src, AVFilterContext sink)
        {
            Graph = graph;
            Src = src;
            Sink = sink;
        }
    }

    internal readonly struct TransformationInfo
    {
        public int InStreamIndex { get; }

        public AVCodecContext DecoderContext { get; }

        public GraphConfiguration? GraphConfiguration { get; }

        public AVCodecContext EncoderContext { get; }

        public int OutStreamIndex { get; }

        public AVRational PreOutTimeBase { get; }

        public TransformationInfo(
            int inStreamIndex,
            AVCodecContext decoderContext,
            GraphConfiguration? graphConfiguration,
            AVCodecContext encoderContext,
            int outStreamIndex,
            AVRational preOutTimeBase)
        {
            InStreamIndex = inStreamIndex;
            DecoderContext = decoderContext ?? throw new ArgumentNullException(nameof(decoderContext));
            GraphConfiguration = graphConfiguration;
            EncoderContext = encoderContext ?? throw new ArgumentNullException(nameof(encoderContext));
            OutStreamIndex = outStreamIndex;
            PreOutTimeBase = preOutTimeBase;
        }
    }

    private static AVPixelFormat ParseVideoPixelFormatSetting(string? name)
        => AVPixelFormatDescriptor.TryParsePixelFormat(name, out var pixelFormat)
            ? pixelFormat
            : AVPixelFormat.AV_PIX_FMT_YUV420P;

    public AVFormatInputContext InCtx { get; }

    public int? VideoStreamIndex { get; }

    public int? AudioStreamIndex { get; }

    public Size Size
    {
        get
        {
            if (VideoStreamIndex is int index)
            {
                var codecParameters = InCtx.Streams[index].CodecParameters;
                return new(codecParameters.Width, codecParameters.Height);
            }
            return default;
        }
    }

    public int Rotation => 0; // FIXME!!!!

    public string? VideoCodec
        => VideoStreamIndex is int index
            ? AVCodec.FindDecoder(InCtx.Streams[index].CodecId).Name ?? string.Empty
            : default;

    public string? AudioCodec => AudioStreamIndex is int index
        ? AVCodec.FindDecoder(InCtx.Streams[index].CodecId).Name ?? string.Empty
        : default;

    public Video(
        AVFormatInputContext inCtx,
        int? videoStreamIndex,
        int? audioStreamIndex)
    {
        InCtx = inCtx;
        VideoStreamIndex = videoStreamIndex;
        AudioStreamIndex = audioStreamIndex;
    }

    public ValueTask DisposeAsync()
        => InCtx.DisposeAsync();

    public ValueTask<VideoInfo> GetVideoInfoAsync(CancellationToken cancellationToken = default)
    {
        // var info = new VideoInfo(
        //     width: VideoStream.CodecParameters.Width,
        //     height: VideoStream.CodecParameters.Height,
        //     duration: TimeSpan.FromSeconds(VideoStream.Duration * VideoStream.TimeBase.ToDouble())
        // );
        // return new(info);
        throw new NotImplementedException();
    }

    private Size GetFinalSize(IReadOnlyList<VideoTransformation> transformations)
    {
        var size = Size;
        foreach (var transformation in transformations)
        {
            size = transformation.GetTargetSize() ?? size;
        }
        return size;
    }

    private AVCodecContext InitializeVideoEncoderContext(
        AVStreamView inStream,
        AVCodecContext decoderContext,
        VideoSettings? videoSettings,
        int quality,
        IReadOnlyList<VideoTransformation> transformations)
    {
        var size = GetFinalSize(transformations);
        var encoderContext = AVCodecContext.CreateEncoderContext("libx264");
        encoderContext.PixelFormat = ParseVideoPixelFormatSetting(videoSettings?.PixelFormat);
        encoderContext.Width = size.Width;
        encoderContext.Height = size.Height;
        encoderContext.CodedWidth = size.Width;
        encoderContext.CodedHeight = size.Height;
        encoderContext.TimeBase = decoderContext.FrameRate switch
        {
            { Numerator: 0 } => encoderContext.Codec.SupportedFramerates switch
            {
                [ var f, .. ] => f,
                _ => inStream.TimeBase
            },
            _ => decoderContext.FrameRate.Reciprocal()
        };
        encoderContext.SampleAspectRatio = new(size.Width, size.Height);
        encoderContext.FieldOrder = AVFieldOrder.AV_FIELD_PROGRESSIVE;
        if (videoSettings is X264Settings settings)
        {
            if (settings.BitRate is long bitRate)
            {
                encoderContext.BitRate = bitRate;
            }
            if (!string.IsNullOrEmpty(settings.Preset))
            {
                encoderContext.SetOption("preset", settings.Preset);
            }
        }
        // CRF --> 0 (best) - 51 (worst)
        var crf = (int)Math.Round((100.0 - (double)Math.Min(100, Math.Max(0, quality))) / 100.0 * 51.0);
        encoderContext.SetOption("crf", crf);
        return encoderContext;
    }

    private GraphConfiguration InitializeVideoGraph(
        AVCodecContext decoderContext,
        AVStreamView istream,
        IReadOnlyList<VideoTransformation> transformations)
    {
        var graph = AVFilterGraph.Alloc();
        // NOTE: disposed by AVFilterGraph
        var buffersrc = graph.CreateBuffer("in", decoderContext, istream);
        // NOTE: disposed by AVFilterGraph
        var buffersink = graph.CreateBufferSink("out");
        // NOTE: disposed by AVFilterGraph
        var txs = new List<AVFilterContext>();
        foreach (var transformation in transformations)
        {
            var tx = transformation.CreateAVFilterContext(graph);
            if (tx is not null)
            {
                txs.Add(tx);
            }
        }
        // LINK ********************************************************************************************************
        for (var i = 0; i < txs.Count; ++i)
        {
            AVFilterContext.Link(
                source: 0 == i ? buffersrc : txs[i - 1],
                sourcePad: 0,
                destination: txs[i],
                destinationPad: 0
            );
        }
        AVFilterContext.Link(
            source: txs[^1],
            sourcePad: 0,
            destination: buffersink,
            destinationPad: 0
        );
        // VALIDATE ****************************************************************************************************
        graph.Configure();
        return new(graph, buffersrc, buffersink);
    }

    private TransformationInfo InitializeVideoTransformation(
        int inStreamIndex,
        VideoSettings? videoSettings,
        int quality,
        IReadOnlyList<VideoTransformation> transformations)
    {
        var inStream = InCtx.Streams[inStreamIndex];
        var timeBase = inStream.TimeBase;
        // VIDEO DECODER CONTEXT ***************************************************************************************
        // NOTE: disposed by Decoder
        var decoderContext = AVCodecContext.CreateDecoderContext(inStream);
        // FILTER GRAPH CONTEXT ****************************************************************************************
        var txs = transformations.WhereRealTransformation().ToList();
        // NOTE: disposed by GraphTransformation
        GraphConfiguration? graphConfiguration;
        if (txs.Count == 0)
        {
            graphConfiguration = default;
        }
        else
        {
            var config = InitializeVideoGraph(decoderContext, inStream, txs);
            timeBase = config.Sink.Inputs[0].TimeBase;
            graphConfiguration = config;
        }
        // VIDEO ENCODER CONTEXT ***************************************************************************************
        // NOTE: disposed by the Encoder
        var encoderContext = InitializeVideoEncoderContext(inStream, decoderContext, videoSettings, quality, txs);
        // CREATE TRANSFORMATION ***************************************************************************************
        return new TransformationInfo(
            inStreamIndex: inStreamIndex,
            decoderContext: decoderContext,
            graphConfiguration: graphConfiguration,
            encoderContext: encoderContext,
            outStreamIndex: 0,
            preOutTimeBase: timeBase
        );
    }

    private AVCodecContext InitializeAudioEncoderContext(AVStreamView inStream)
    {
        var encoderContext = AVCodecContext.CreateEncoderContext("aac");
        encoderContext.TimeBase = inStream.TimeBase;
        encoderContext.OverwriteChannelLayout(inStream.CodecParameters.ChannelLayout);
        encoderContext.SampleFormat = encoderContext.Codec.SampleFormats[0];
        encoderContext.SampleRate = inStream.CodecParameters.SampleRate;
        encoderContext.FieldOrder = AVFieldOrder.AV_FIELD_PROGRESSIVE;
        return encoderContext;
    }

    private TransformationInfo InitializeAudioTransformation(
        int inStreamIndex,
        bool hasVideo)
    {
        var inStream = InCtx.Streams[inStreamIndex];
        var timeBase = inStream.TimeBase;
        // AUDIO DECODER CONTEXT ***************************************************************************************
        // NOTE: disposed by Decoder
        var decoderContext = AVCodecContext.CreateDecoderContext(inStream);
        // AUDIO ENCODER CONTEXT ***************************************************************************************
        // NOTE: disposed by the Encoder
        var encoderContext = InitializeAudioEncoderContext(inStream);
        // CREATE TRANSFORMATION ***************************************************************************************
        return new TransformationInfo(
            inStreamIndex: inStreamIndex,
            decoderContext: decoderContext,
            graphConfiguration: default,
            encoderContext: encoderContext,
            outStreamIndex: hasVideo ? 1 : 0,
            preOutTimeBase: timeBase
        );
    }

    private Decoder CreateTransformation(TransformationInfo transformation, IConsumer<AVPacket> consumer)
    {
        // NOTE: disposed either by the Decoder or by the Graph
        var encoder = new Encoder(transformation.EncoderContext, transformation.OutStreamIndex, consumer);
        // NOTE: disposed by the Demuxer
        return new Decoder(
            decoderCtx: transformation.DecoderContext,
            // NOTE: disposed by Decoder
            consumer: transformation.GraphConfiguration is GraphConfiguration videoConf
                ? new GraphTransformation(
                    graph: videoConf.Graph,
                    inBuffer: videoConf.Src,
                    outBuffer: videoConf.Sink,
                    consumer: encoder
                )
                : encoder
        );
    }

    public void WriteTo(
        Stream stream,
        IReadOnlyList<VideoTransformation> transformations,
        VideoSettings? videoSettings,
        string? audioType,
        int quality = 85,
        bool optimize = true,
        CancellationToken cancellationToken = default)
    {
        var v = VideoStreamIndex is int videoStreamIndex
            ? InitializeVideoTransformation(videoStreamIndex, videoSettings, quality, transformations)
            : default(TransformationInfo?);
        var a = AudioStreamIndex is int audioStreamIndex && audioType != "none"
            ? InitializeAudioTransformation(audioStreamIndex, v.HasValue)
            : default(TransformationInfo?);

        // FORMAT OUTPUT CONTEXT ***************************************************************************************
        // NOTE: disposed by OutputWriter
        var outCtx = AVFormatOutputContext.CreateOutputContext(
            format: AVOutputFormat.Guess(filename: "out.mp4"),
            outputStream: stream
        );
        if (v is TransformationInfo vt0)
        {
            if (outCtx.Flags.HasFlag(AVFormatFlags.AVFMT_GLOBALHEADER))
            {
                vt0.EncoderContext.Flags |= AVCodecFlags.AV_CODEC_FLAG_GLOBAL_HEADER;
            }
            var inStream = InCtx.Streams[vt0.InStreamIndex];
            var outStream = outCtx.NewStream(vt0.EncoderContext);
            outStream.Duration = inStream.Duration;
            outStream.TimeBase = vt0.EncoderContext.TimeBase;
            outStream.StartTime = inStream.StartTime;
        }
        if (a is TransformationInfo at0)
        {
            var inStream = InCtx.Streams[at0.InStreamIndex];
            var outStream = outCtx.NewStream(at0.EncoderContext);
            outStream.Duration = inStream.Duration;
            outStream.TimeBase = inStream.TimeBase;
            outStream.StartTime = inStream.StartTime;
        }
        // BUILD PIPELINE **********************************************************************************************
        // INPUT -> DEMUXER -> (DECODER -> GRAPH? -> ENCODER)+ -> MUXER -> OUTPUT
        // OUTPUT WRITER **********************************************************************************************
        // NOTE: disposed by the Muxer
        var outStream0 = outCtx.Streams[0];
        var outputWriter = new OutputWriter(
            outCtx: outCtx,
            sourceTimeBases: new Dictionary<int, AVRational> { v, a }
        );
        // MUXER *******************************************************************************************************
        // NOTE: disposed by the Encoder
        var muxer = new Muxer(outputWriter);
        // VIDEO *******************************************************************************************************
        // NOTE: disposed by Demuxer
        var videoDecoder = v is TransformationInfo vt
            ? CreateTransformation(vt, muxer)
            : default;
        // AUDIO *******************************************************************************************************
        // NOTE: disposed by Demuxer
        var audioDecoder = a is TransformationInfo at
            ? CreateTransformation(at, muxer)
            : default;
        // DEMUXER *****************************************************************************************************
        using var demuxer = new Demuxer(new Dictionary<int, IConsumer<AVPacket>>
        {
            { v?.InStreamIndex, videoDecoder },
            { a?.InStreamIndex, audioDecoder },
        });
        // EXECUTE PIPELINE ********************************************************************************************
        cancellationToken.ThrowIfCancellationRequested();
        using var packet = AVPacket.CreatePacket();
        while (InCtx.ReadFrame(packet))
        {
            cancellationToken.ThrowIfCancellationRequested();
            demuxer.Consume(packet);
            packet.Unref();
        }
        // FLUSH BUFFERS ***********************************************************************************************
        demuxer.Flush();
        stream.Flush();
    }

    public ValueTask WriteToAsync(
        Stream stream,
        IReadOnlyList<VideoTransformation> transformations,
        VideoSettings? videoSettings,
        string? audioType,
        int quality = 85,
        bool optimize = true,
        CancellationToken cancellationToken = default)
    {
        WriteTo(
            stream,
            transformations,
            videoSettings,
            audioType,
            quality,
            optimize,
            cancellationToken
        );
        return default;
    }

    private GraphConfiguration InitializeThumbnailGraph(
        AVCodecContext decoderContext,
        AVStreamView istream)
    {
        var graph = AVFilterGraph.Alloc();
        // NOTE: disposed by AVFilterGraph
        var buffersrc = graph.CreateBuffer("in", decoderContext, istream);
        // NOTE: disposed by AVFilterGraph
        var buffersink = graph.CreateBufferSink("out");
        // NOTE: disposed by AVFilterGraph
        var format = AVFilterContext.Create(
            filterName: "format",
            name: "format",
            graph: graph,
            "pix_fmts=yuvj422p"
        );
        AVFilterContext.Link(buffersrc, 0, format, 0);
        AVFilterContext.Link(format, 0, buffersink, 0);
        // VALIDATE ****************************************************************************************************
        graph.Configure();
        return new(graph, buffersrc, buffersink);
    }

    private TransformationInfo InitializeThumbnailTransformation(int inStreamIndex)
    {
        var inStream = InCtx.Streams[inStreamIndex];
        var timeBase = inStream.TimeBase;
        // VIDEO DECODER CONTEXT ***************************************************************************************
        // NOTE: disposed by Decoder
        var decoderContext = AVCodecContext.CreateDecoderContext(inStream);
        // FILTER GRAPH CONTEXT ****************************************************************************************
        // NOTE: disposed by GraphTransformation
        var graphConfiguration = InitializeThumbnailGraph(decoderContext, inStream);
        // VIDEO ENCODER CONTEXT ***************************************************************************************
        // NOTE: disposed by the Encoder
        using var encoderContext = AVCodecContext.CreateEncoderContext("mjpeg");
        encoderContext.Width = decoderContext.Width;
        encoderContext.Height = decoderContext.Height;
        encoderContext.CodedWidth = decoderContext.Width;
        encoderContext.CodedHeight = decoderContext.Height;
        encoderContext.TimeBase = inStream.TimeBase;
        encoderContext.PixelFormat = AVPixelFormat.AV_PIX_FMT_YUVJ422P;
        encoderContext.FieldOrder = decoderContext.FieldOrder;
        encoderContext.Flags |= AVCodecFlags.AV_CODEC_FLAG_QSCALE;
        encoderContext.GlobalQuality = /* FF_QP2LAMBDA */ 118 * 21;
        encoderContext.Debug = true;
        // CREATE TRANSFORMATION ***************************************************************************************
        return new TransformationInfo(
            inStreamIndex: inStreamIndex,
            decoderContext: decoderContext,
            graphConfiguration: graphConfiguration,
            encoderContext: encoderContext,
            outStreamIndex: 0,
            preOutTimeBase: timeBase
        );
    }

    private void WriteThumbnail(Stream stream, TimeSpan captureTime, CancellationToken cancellationToken = default)
    {
        if (VideoStreamIndex is not int inStreamIndex)
        {
            throw new NoVideoStreamException();
        }
        var inStream = InCtx.Streams[inStreamIndex];
        var timeBase = inStream.TimeBase;
        // FORMAT OUTPUT CONTEXT **************************************************************************************
        using var outCtx = AVFormatOutputContext.CreateOutputContext(
            format: AVOutputFormat.Guess(shortname: "image2pipe"),
            outputStream: stream
            // filename: "/tmp/out.jpg"
        );
        // VIDEO DECODER CONTEXT ***************************************************************************************
        using var decoderContext = AVCodecContext.CreateDecoderContext(inStream);
        // IMAGE ENCODER CONTEXT ***************************************************************************************
        using var encoderContext = AVCodecContext.CreateEncoderContext("mjpeg");
        encoderContext.Width = decoderContext.Width;
        encoderContext.Height = decoderContext.Height;
        encoderContext.CodedWidth = decoderContext.Width;
        encoderContext.CodedHeight = decoderContext.Height;
        encoderContext.TimeBase = inStream.TimeBase;
        encoderContext.PixelFormat = AVPixelFormat.AV_PIX_FMT_YUVJ422P;
        encoderContext.FieldOrder = decoderContext.FieldOrder;
        encoderContext.Flags |= AVCodecFlags.AV_CODEC_FLAG_QSCALE;
        encoderContext.GlobalQuality = /* FF_QP2LAMBDA */ 118 * 21;
        encoderContext.Debug = true;
        // GRAPH *******************************************************************************************************
        var gi = InitializeThumbnailGraph(decoderContext, inStream);
        var outStream = outCtx.NewStream(encoderContext);
        decoderContext.Open();
        encoderContext.Open();
        cancellationToken.ThrowIfCancellationRequested();
        using var inFrame = AVFrame.CreateFrame();
        using var outFrame = AVFrame.CreateFrame();
        using var inPacket = AVPacket.CreatePacket();
        using var outPacket = AVPacket.CreatePacket();
        using var bsfPacket = AVPacket.CreatePacket();
        var shouldStop = false;
        while (!shouldStop && InCtx.ReadFrame(inPacket))
        {
            if (inStreamIndex == inPacket.StreamIndex)
            {
                cancellationToken.ThrowIfCancellationRequested();
                decoderContext.SendPacket(inPacket);
                while (!shouldStop && decoderContext.TryReceiveFrame(inFrame))
                {
                    var ts = inFrame.GetPresentationTimestamp(inStream);
                    if (captureTime.Ticks <= ts.Ticks && inFrame.PictureType == AVPictureType.AV_PICTURE_TYPE_I)
                    {
                        gi.Src.BufferSrcAddFrame(inFrame);
                        if (!gi.Sink.BufferSinkGetFrame(outFrame))
                        {
                            throw new InvalidOperationException("Failed to transform frame.");
                        }
                        outCtx.WriteHeader();
                        encoderContext.SendFrame(outFrame);
                        while (encoderContext.TryReceivePacket(outPacket))
                        {
                            using var mjpeg2jpeg = AVBSFContext.AllocContext("mjpeg2jpeg");
                            mjpeg2jpeg.CopyInParametersFrom(outStream.CodecParameters);
                            mjpeg2jpeg.Init();
                            mjpeg2jpeg.SendPacket(outPacket);
                            if (!mjpeg2jpeg.TryReceivePacket(bsfPacket))
                            {
                                throw new InvalidOperationException("Failed to receive packet from mjpeg2jpeg.");
                            }
                            bsfPacket.StreamIndex = outStream.Index;
                            bsfPacket.Position = -1;
                            outCtx.InterleavedWriteFrame(bsfPacket);
                            bsfPacket.Unref();
                            mjpeg2jpeg.SendFlushPacket();
                            outPacket.Unref();
                        }
                        outFrame.Unref();
                        outCtx.InterleavedFlush();
                        outCtx.WriteTrailer();
                        shouldStop = true;
                        break;
                    }
                    inFrame.Unref();
                }
            }
            inPacket.Unref();
        }
    }

    public ValueTask WriteThumbnailAsync(Stream stream, TimeSpan captureTime, CancellationToken cancellationToken = default)
    {
        WriteThumbnail(stream, captureTime, cancellationToken);
        return default;
    }
}
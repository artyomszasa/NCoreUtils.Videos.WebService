using NCoreUtils.FFMpeg;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos.FFMpeg;

public static class VideoTransformationExtensions
{
    private readonly struct Unit { }

    private sealed class GetTargetSizeVisitor : IVideoTransformationVisitor<Unit, Size?>
    {
        public static GetTargetSizeVisitor Singleton { get; } = new();

        private GetTargetSizeVisitor() { /* noop */ }

        public Size? VisitCrop(Unit arg, Rectangle rect)
            => rect.Size;

        public Size? VisitNoop(Unit arg)
            => default;

        public Size? VisitResize(Unit arg, Size size)
            => size;
    }

    private sealed class CreateAVFilterContextVisitor : IVideoTransformationVisitor<AVFilterGraph, AVFilterContext?>
    {
        public static CreateAVFilterContextVisitor Singleton { get; } = new();

        private CreateAVFilterContextVisitor() { /* noop */ }

        public AVFilterContext? VisitCrop(AVFilterGraph arg, Rectangle rect)
            => arg.CreateCrop("crop", rect.Width, rect.Height, rect.X, rect.Y);

        public AVFilterContext? VisitNoop(AVFilterGraph arg)
            => default;

        public AVFilterContext? VisitResize(AVFilterGraph arg, Size size)
            => arg.CreateScale("resize", size.Width, size.Height);
    }

    private sealed class IsRealTransformationVisitor : IVideoTransformationVisitor<Unit, bool>
    {
        public static IsRealTransformationVisitor Singleton { get; } = new();

        private IsRealTransformationVisitor() { /* noop */ }

        public bool VisitCrop(Unit arg, Rectangle rect)
            => true;

        public bool VisitNoop(Unit arg)
            => false;

        public bool VisitResize(Unit arg, Size size)
            => true;
    }

    public static Size? GetTargetSize(in this VideoTransformation videoTransformation)
        => videoTransformation.Accept(GetTargetSizeVisitor.Singleton, default);

    public static AVFilterContext? CreateAVFilterContext(in this VideoTransformation videoTransformation, AVFilterGraph graph)
        => videoTransformation.Accept(CreateAVFilterContextVisitor.Singleton, graph);

    public static bool IsRealTransformation(in this VideoTransformation videoTransformation)
        => videoTransformation.Accept(IsRealTransformationVisitor.Singleton, default);

    public static IEnumerable<VideoTransformation> WhereRealTransformation(this IEnumerable<VideoTransformation> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        foreach (var tx in source)
        {
            if (tx.IsRealTransformation())
            {
                yield return tx;
            }
        }
    }
}
namespace NCoreUtils.Videos.Internal;

public interface IVideoTransformationVisitor<TArg, TResult>
{
    TResult VisitNoop(TArg arg);

    TResult VisitResize(TArg arg, Size size);

    TResult VisitCrop(TArg arg, Rectangle rect);
}
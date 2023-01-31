namespace NCoreUtils.Videos.Internal;

public interface IResizerFactory
{
    IResizer CreateResizer(IVideo video, ResizeOptions options);
}
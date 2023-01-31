namespace NCoreUtils.Videos;

public interface IVideoResizerOptions
{
    int Quality(string videoType);

    bool Optimize(string videoType);
}
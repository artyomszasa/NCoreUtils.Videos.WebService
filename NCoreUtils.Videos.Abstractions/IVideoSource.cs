using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public interface IVideoSource
    {
        bool Reusable { get; }

        IStreamProducer CreateProducer();
    }
}
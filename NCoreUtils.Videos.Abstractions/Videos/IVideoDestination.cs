using NCoreUtils.Images;
using NCoreUtils.IO;

namespace NCoreUtils.Videos
{
    public interface IVideoDestination
    {
        IStreamConsumer CreateConsumer(ContentInfo contentInfo);
    }
}
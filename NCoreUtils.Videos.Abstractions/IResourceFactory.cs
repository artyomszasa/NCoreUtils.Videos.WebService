using System;

namespace NCoreUtils.Videos
{
    public interface IResourceFactory
    {
        IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next);

        IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next);
    }
}
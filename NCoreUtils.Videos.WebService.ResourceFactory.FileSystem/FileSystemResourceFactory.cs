using System;

namespace NCoreUtils.Videos.WebService
{
    public class FileSystemResourceFactory : IResourceFactory
    {
        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            if (uri is not null && uri.Scheme == "file")
            {
                return new FileSystemDestination(uri.AbsolutePath);
            }
            return next();
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            if (uri is not null && uri.Scheme == "file")
            {
                return new FileSystemSource(uri.AbsolutePath);
            }
            return next();
        }
    }
}
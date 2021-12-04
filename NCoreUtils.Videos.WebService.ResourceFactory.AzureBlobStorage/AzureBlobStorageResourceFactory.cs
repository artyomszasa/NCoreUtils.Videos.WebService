using System;

namespace NCoreUtils.Videos.WebService
{
    public class AzureBlobStorageResourceFactory : IResourceFactory
    {
        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            if (uri is null || uri.Scheme != "az")
            {
                return next();
            }
            return new AzureBlobStorageDestination(uri);
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            if (uri is null || uri.Scheme != "az")
            {
                return next();
            }
            return new AzureBlobStorageSource(uri);
        }
    }
}
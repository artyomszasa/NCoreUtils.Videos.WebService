using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCoreUtils.Videos
{
    public class GoogleCloudStorageResourceFactory : IResourceFactory
    {
        public GoogleCloudStorageResourceFactory()
        {
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            throw new NotImplementedException();
        }

        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            throw new NotImplementedException();
        }
    }
}
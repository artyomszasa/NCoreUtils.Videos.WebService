using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace NCoreUtils.Videos.WebService
{
    public class Startup : CoreStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env) { }

        protected override void ConfigureResourceFactories(CompositeResourceFactoryBuilder b)
        {
            b
                // inline data
                .Add<DefaultResourceFactory>()
                // GCS
                .Add<GoogleCloudStorageResourceFactory>()
                // Azure Bloc Storage
                .Add<AzureBlobStorageResourceFactory>()
                // locally mounted fs
                .Add<FileSystemResourceFactory>();
        }
    }
}
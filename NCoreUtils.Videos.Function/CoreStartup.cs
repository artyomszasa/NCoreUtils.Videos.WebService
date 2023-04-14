using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

namespace NCoreUtils.Videos.Function;

internal class Startup : Generic.CoreStartup
{
    public Startup(IConfiguration configuration)
        : base(configuration)
    { /* noop */ }

    protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
    {
        b.AddAzureBlobResourceFactory();
    }

    public void Configure(IServiceProvider serviceProvider)
        => base.ConfigureBase(serviceProvider);
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NCoreUtils.Videos.Function;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables("VIDEOS")
    .Build();

var startup = new Startup(configuration);

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((_, services) => startup.ConfigureServices(services))
    .Build();

startup.Configure(host.Services);

host.Run();

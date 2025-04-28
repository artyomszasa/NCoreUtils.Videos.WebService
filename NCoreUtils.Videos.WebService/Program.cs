using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace NCoreUtils.Videos;

public class Program
{
    private static IConfiguration CreateDefaultConfiguration()
        => new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("VIDEOS_")
            .Build();

    public static void Main(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
        {
            null or "" => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") switch
            {
                null or "" => "Development",
                string dotnetEnv => dotnetEnv
            },
            string aspNetCoreEnv => aspNetCoreEnv
        };

        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName,
            ContentRootPath = Environment.CurrentDirectory
        });
        var configuration = CreateDefaultConfiguration();
        builder.Configuration.AddConfiguration(configuration);
        builder.Logging.ConfigureWebServiceLogging(configuration);
        builder.UseMinimalKestrel();
        var startup = new Startup(configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);
        var app = builder.Build();
        startup.Configure(app.Services, app);
        app.Run();
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if EnableGoogleFluentdLogging
using NCoreUtils.Logging;
#endif

namespace NCoreUtils.Videos.WebService
{
    public class Program
    {
        static IPEndPoint ParseEndpoint(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new IPEndPoint(IPAddress.Loopback, 5000);
            }
            var portIndex = input.LastIndexOf(':');
            if (-1 == portIndex)
            {
                return new IPEndPoint(IPAddress.Parse(input), 5000);
            }
            else
            {
                return new IPEndPoint(IPAddress.Parse(input.AsSpan(0, portIndex)), int.Parse(input.AsSpan()[(portIndex + 1)..]));
            }
        }

        private static IConfigurationRoot LoadConfiguration()
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("VIDEOS_")
                .Build();

#if EnableGoogleCloudStorage
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(global::Google.Apis.Auth.OAuth2.JsonCredentialParameters))]
#endif
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

#pragma warning disable IDE0060
        public static IHostBuilder CreateHostBuilder(string[] args)
#pragma warning restore IDE0060
        {
            var configuration = LoadConfiguration();
            return new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .ConfigureAppConfiguration(b => b.AddConfiguration(configuration))
                .ConfigureLogging((ctx, builder) =>
                {
                    builder
                        .ClearProviders()
                        .AddConfiguration(configuration)
#if EnableGoogleFluentdLogging
                        .AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"]);
#else
                        .AddConsole();
#endif
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o =>
                    {
                        // Google Cloud Run passes port to listen on through PORT env variable.
                        var endpoint = Environment.GetEnvironmentVariable("PORT") switch
                        {
                            null => ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN")),
                            string portAsString => int.TryParse(portAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
                                ? new IPEndPoint(IPAddress.Any, port)
                                : ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN"))
                        };
                        o.Listen(endpoint);
                        o.AllowSynchronousIO = true;
                    });
                });
        }
    }
}

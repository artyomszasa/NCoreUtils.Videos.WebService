using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#if EnableGoogleFluentdLogging
using NCoreUtils.Logging;
#endif

namespace NCoreUtils.Videos;

internal static class StartupExtensions
{
    public static void ConfigureWebServiceLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder
            .ClearProviders()
            .AddConfiguration(configuration.GetSection("Logging"))
#if EnableGoogleFluentdLogging
            .AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"]);
#else
            .AddConsole();
#endif
    }

    public static WebApplicationBuilder UseMinimalKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrelCore();
        builder.WebHost.ConfigureKestrel(opts =>
        {
            var port = Environment.GetEnvironmentVariable("PORT") switch
            {
                null or "" => 5000,
                var rawPort => int.TryParse(rawPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out var portValue)
                    ? portValue
                    : throw new InvalidOperationException($"PORT environment variable expected to be an integer found \"{rawPort}\".")
            };
            opts.ListenAnyIP(port);
        });
        return builder;
    }
}
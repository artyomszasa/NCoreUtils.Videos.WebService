using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#if EnableGoogleFluentdLogging
using NCoreUtils.Logging;
#endif

namespace NCoreUtils.Videos;

internal static class StartupExtensions
{
    private static void ConfigureJobLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder
            .ClearProviders()
            .AddConfiguration(configuration.GetSection("Logging"))
#if EnableGoogleFluentdLogging
            .AddGoogleFluentd(projectId: configuration["Google:ProjectId"]);
#else
            .AddSimpleConsole(o => o.SingleLine = true);
#endif
    }

    public static IVideoResizerOptions GetVideoResizerOptions(this IConfigurationSection section)
    {
        var options = new VideoResizerOptions();
        var rawMemoryLimit = section[nameof(VideoResizerOptions.MemoryLimit)];
        if (rawMemoryLimit is not null)
        {
            if (long.TryParse(rawMemoryLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out var memoryLimit))
            {
                options.MemoryLimit = memoryLimit;
            }
            else
            {
                throw new InvalidOperationException($"Invalid value for Videos:{nameof(VideoResizerOptions.MemoryLimit)}: \"{rawMemoryLimit}\".");
            }
        }
        foreach (var (key, value) in section.GetSection(nameof(VideoResizerOptions.Quality)).AsEnumerable())
        {
            if (value is not null)
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ivalue))
                {
                    options.Quality[key] = ivalue;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid value for Videos:{nameof(VideoResizerOptions.Quality)}:{key}: \"{value}\".");
                }
            }
        }
        foreach (var (key, value) in section.GetSection(nameof(VideoResizerOptions.Optimize)).AsEnumerable())
        {
            if (value is not null)
            {
                if (bool.TryParse(value, out var bvalue))
                {
                    options.Optimize[key] = bvalue;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid value for Videos:{nameof(VideoResizerOptions.Optimize)}:{key}: \"{value}\".");
                }
            }
        }
        return options;
    }

    public static IServiceCollection AddJobLogging(this IServiceCollection services, IConfiguration configuration)
        => services.AddLogging(b => b.ConfigureJobLogging(configuration));

    public static IServiceCollection AddResourceFactories(this IServiceCollection services)
        => services.AddCompositeResourceFactory(b => b
#if EnableGoogleCloudStorage
            // GCS
            .AddGoogleCloudStorageResourceFactory(passthrough: false)
#endif
#if EnableAzureBlobStorage
            // Azure Bloc Storage
            .AddAzureBlobResourceFactory()
#endif
        );
}
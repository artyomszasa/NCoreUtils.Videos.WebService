using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

#if NET7_0_OR_GREATER
using NCoreUtils.FFMpeg;
#endif

namespace NCoreUtils.Videos.Generic;

public abstract class CoreStartup
{
    protected readonly IConfiguration Configuration;

    protected CoreStartup(IConfiguration configuration)
        => Configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));

    protected virtual IVideoResizerOptions GetVideoResizerOptions()
    {
        var section = Configuration.GetSection("Videos");
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

    protected abstract void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b);

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            // video resizer options
            .AddSingleton(GetVideoResizerOptions())
            // Video resizer implementation
#if NET7_0_OR_GREATER
            .AddFFMpegVideoResizer()
#else
            .AddXabeVideoResizer()
#endif
            // source/destination handlers
            .AddCompositeResourceFactory(ConfigureResourceFactories);
    }

    public void ConfigureBase(IServiceProvider serviceProvider)
    {
#if NET7_0_OR_GREATER
        AVLogging.LogLevel = AVLogLevel.AV_LOG_INFO;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        AVLogging.SetLogger(loggerFactory);
#endif
    }
}
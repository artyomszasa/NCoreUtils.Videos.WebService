using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos;
using NCoreUtils.Videos.FFMpeg;

namespace NCoreUtils;

public static class ServiceCollectionFFMpegVideoExtensions
{
    public static IServiceCollection AddFFMpegVideoResizer(
        this IServiceCollection services,
        bool suppressDefaultResizers,
        Action<ResizerCollectionBuilder>? configure)
        => services.AddVideoResizer<VideoProvider>(suppressDefaultResizers, configure);

    public static IServiceCollection AddFFMpegVideoResizer(
        this IServiceCollection services,
        IVideoProcessorConfiguration? configuration,
        bool suppressDefaultResizers = false,
        Action<ResizerCollectionBuilder>? configure = default)
        => services
            .AddSingleton(configuration ?? VideoProcessorConfiguration.Default)
            .AddFFMpegVideoResizer(suppressDefaultResizers, configure);

    public static IServiceCollection AddFFMpegVideoResizer(
        this IServiceCollection services,
        bool? singleThread = default,
        bool suppressDefaultResizers = false,
        Action<ResizerCollectionBuilder>? configure = default)
        => services.AddFFMpegVideoResizer(
            new VideoProcessorConfiguration(singleThread ?? VideoProcessorConfiguration.DefaultSingleThread),
            suppressDefaultResizers,
            configure
        );
}


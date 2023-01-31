using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos;
using NCoreUtils.Videos.FFMpeg;

namespace NCoreUtils;

public static class ServiceCollectionFFMpegVideoExtensions
{
    public static IServiceCollection AddFFMpegVideoResizer(
        this IServiceCollection services,
        bool suppressDefaultResizers = false,
        Action<ResizerCollectionBuilder>? configure = default)
        => services.AddVideoResizer<VideoProvider>(suppressDefaultResizers, configure);
}


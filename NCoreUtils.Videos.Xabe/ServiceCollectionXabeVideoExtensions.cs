using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos.Internal;
using NCoreUtils.Videos.Xabe;

namespace NCoreUtils.Videos
{
    public static class ServiceCollectionXabeVideoExtensions
    {
        public static IServiceCollection AddXabeVideoResizer(
            this IServiceCollection services,
            bool suppressDefaultResizers = false,
            Action<ResizerCollectionBuilder>? configure = default)
            => services
                .AddSingleton<IVideoProvider, VideoProvider>()
                .AddVideoResizer<VideoProvider>(suppressDefaultResizers, configure);
    }
}
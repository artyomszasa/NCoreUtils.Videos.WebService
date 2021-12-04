using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos
{
    public static class ServiceCollectionVideosExtensions
    {
        public static IServiceCollection AddVideoResizer<TProvider>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime,
            bool suppressDefaultResizers = false,
            Action<ResizerCollectionBuilder>? configure = default)
            where TProvider : class, IVideoProvider
        {
            var builder = new ResizerCollectionBuilder();
            if (!suppressDefaultResizers)
            {
                builder
                    .Add(ResizeModes.None, new NoneResizerFactory())
                    .Add(ResizeModes.Exact, ExactResizerFactory.Instance)
                    .Add(ResizeModes.Inbox, InboxResizerFactory.Instance);
            }
            configure?.Invoke(builder);
            services
                .AddSingleton(builder.Build())
                .AddSingleton<IVideoProvider, TProvider>();
            switch (serviceLifetime)
            {
                case ServiceLifetime.Transient:
                    services
                        .AddTransient<VideoResizer>()
                        .AddTransient<IVideoResizer, VideoResizer>()
                        .AddTransient<IVideoAnalyzer, VideoResizer>();
                    break;
                case ServiceLifetime.Scoped:
                    services
                        .AddScoped<VideoResizer>()
                        .AddScoped<IVideoResizer>(serviceProvider => serviceProvider.GetRequiredService<VideoResizer>())
                        .AddScoped<IVideoAnalyzer>(serviceProvider => serviceProvider.GetRequiredService<VideoResizer>());
                    break;
                case ServiceLifetime.Singleton:
                    services
                        .AddSingleton<VideoResizer>()
                        .AddSingleton<IVideoResizer>(serviceProvider => serviceProvider.GetRequiredService<VideoResizer>())
                        .AddSingleton<IVideoAnalyzer>(serviceProvider => serviceProvider.GetRequiredService<VideoResizer>());
                    break;
            }
            return services;
        }

        public static IServiceCollection AddVideoResizer<TProvider>(
            this IServiceCollection services,
            bool suppressDefaultResizers = false,
            Action<ResizerCollectionBuilder>? configure = default)
            where TProvider : class, IVideoProvider
            => services.AddVideoResizer<TProvider>(ServiceLifetime.Singleton, suppressDefaultResizers, configure);
    }
}
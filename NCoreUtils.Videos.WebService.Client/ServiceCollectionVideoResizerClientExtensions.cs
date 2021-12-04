using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils
{
    public static class ServiceCollectionVideoResizerClientExtensions
    {
        public static IServiceCollection AddVideoResizerClient(
            this IServiceCollection services,
            VideosClientConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (string.IsNullOrEmpty(configuration.EndPoint))
            {
                throw new InvalidOperationException("Video resizer client endpoint must not be empty");
            }
            return services
                .AddSingleton(configuration.AsTyped<VideoResizerClient>())
                .AddSingleton<IVideoResizer, VideoResizerClient>();
        }

        public static IServiceCollection AddVideoResizerClient(
            this IServiceCollection services,
            string endpoint,
            bool allowInlineData = false,
            bool cacheCapabilities = true,
            string httpClient = VideosClientConfiguration.DefaultHttpClient)
            => services.AddVideoResizerClient(new VideosClientConfiguration
            {
                EndPoint = endpoint,
                AllowInlineData = allowInlineData,
                CacheCapabilities = cacheCapabilities,
                HttpClient = httpClient
            });

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(VideosClientConfiguration))]
#endif
        public static IServiceCollection AddVideoResizerClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var conf = new VideosClientConfiguration();
            configuration.Bind(conf);
            return services.AddVideoResizerClient(conf);
        }

        public static IServiceCollection AddVideoAnalyzerClient(
            this IServiceCollection services,
            VideosClientConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (string.IsNullOrEmpty(configuration.EndPoint))
            {
                throw new InvalidOperationException("Video analyzer client endpoint must not be empty");
            }
            return services
                .AddSingleton(configuration.AsTyped<VideoAnalyzerClient>())
                .AddSingleton<IVideoAnalyzer, VideoAnalyzerClient>();
        }

        public static IServiceCollection AddVideoAnalyzerClient(
            this IServiceCollection services,
            string endpoint,
            bool allowInlineData = false,
            bool cacheCapabilities = true,
            string httpClient = VideosClientConfiguration.DefaultHttpClient)
            => services.AddVideoAnalyzerClient(new VideosClientConfiguration
            {
                EndPoint = endpoint,
                AllowInlineData = allowInlineData,
                CacheCapabilities = cacheCapabilities,
                HttpClient = httpClient
            });

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(VideosClientConfiguration))]
#endif
        public static IServiceCollection AddVideoAnalyzerClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var conf = new VideosClientConfiguration();
            configuration.Bind(conf);
            return services.AddVideoAnalyzerClient(conf);
        }
    }
}
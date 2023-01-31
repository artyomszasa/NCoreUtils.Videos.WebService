using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Videos;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils;

public static class ServiceCollectionVideoResizerClientExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            var path = configuration is IConfigurationSection section ? $"{section.Path}:{key}" : key;
            throw new InvalidOperationException($"No required value found at {path}");
        }
        return value;
    }

    private static void BindVideosClientConfiguration(IConfiguration configuration, VideosClientConfiguration config)
    {
        config.EndPoint = configuration.GetRequiredValue(nameof(config.EndPoint));
        var httpClient = configuration[nameof(config.HttpClient)];
        if (!string.IsNullOrEmpty(httpClient))
        {
            config.HttpClient = httpClient;
        }
        var rawAllowInlineData = configuration[nameof(config.AllowInlineData)];
        if (!string.IsNullOrEmpty(rawAllowInlineData))
        {
            config.AllowInlineData = bool.Parse(rawAllowInlineData);
        }
        var rawCacheCapabilities = configuration[nameof(config.CacheCapabilities)];
        if (!string.IsNullOrEmpty(rawCacheCapabilities))
        {
            config.CacheCapabilities = bool.Parse(rawCacheCapabilities);
        }
    }

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

    public static IServiceCollection AddVideoResizerClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        var conf = new VideosClientConfiguration();
        BindVideosClientConfiguration(configuration, conf);
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

    public static IServiceCollection AddVideoAnalyzerClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        var conf = new VideosClientConfiguration();
        BindVideosClientConfiguration(configuration, conf);
        return services.AddVideoAnalyzerClient(conf);
    }
}
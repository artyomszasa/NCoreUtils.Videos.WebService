using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

namespace NCoreUtils.Videos.WebService;

public abstract class CoreStartup
{
    private sealed class ConfigureJson : IConfigureOptions<JsonSerializerOptions>
    {
        public void Configure(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
    }

    private static ForwardedHeadersOptions ConfigureForwardedHeaders()
    {
        var opts = new ForwardedHeadersOptions();
        opts.KnownNetworks.Clear();
        opts.KnownProxies.Clear();
        opts.ForwardedHeaders = ForwardedHeaders.All;
        return opts;
    }

    private readonly IConfiguration Configuration;

    private readonly IWebHostEnvironment? Env;

    protected CoreStartup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Env = env ?? throw new ArgumentNullException(nameof(env));
    }

    protected virtual void AddHttpContextAccessor(IServiceCollection services)
    {
        services
            .AddHttpContextAccessor();
    }

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
        AddHttpContextAccessor(services);

        services
            // JSON Serialization
            .AddOptions<JsonSerializerOptions>()
                .Configure(opts => opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                .Services
            // video resizer options
            .AddSingleton(GetVideoResizerOptions())
            // http client factory
            .AddHttpClient()
            // Video resizer implementation
#if NET7_0_OR_GREATER
            .AddFFMpegVideoResizer()
#else
            .AddXabeVideoResizer()
#endif
            // CORS
            .AddCors(b => b.AddDefaultPolicy(opts => opts
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                // must be at least 2 domains for CORS middleware to send Vary: Origin
                .WithOrigins("https://example.com", "http://127.0.0.1")
                .SetIsOriginAllowed(_ => true)
            ))
            // source/destination handlers
            .AddCompositeResourceFactory(b =>
            {
                ConfigureResourceFactories(b.AddAspNetCoreResourceFactory());
            });
    }

    public virtual void Configure(IApplicationBuilder app)
    {
#if DEBUG
        if (Env is not null && Env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
#endif

        app
            .UseForwardedHeaders(ConfigureForwardedHeaders())
            .UseCors()
            .UseMiddleware<ErrorMiddleware>()
            .UseMiddleware<VideosMiddleware>()
            .Run((context) =>
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });
    }
}
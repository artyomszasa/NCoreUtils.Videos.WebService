using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

#if NET7_0_OR_GREATER
using NCoreUtils.FFMpeg;
#endif

namespace NCoreUtils.Videos.WebService;

public abstract class CoreStartup : Generic.CoreStartup
{
    private static ForwardedHeadersOptions ConfigureForwardedHeaders()
    {
        var opts = new ForwardedHeadersOptions();
        opts.KnownNetworks.Clear();
        opts.KnownProxies.Clear();
        opts.ForwardedHeaders = ForwardedHeaders.All;
        return opts;
    }

    protected readonly IWebHostEnvironment? Env;

    protected CoreStartup(IConfiguration configuration, IWebHostEnvironment env)
        : base(configuration)
        => Env = env ?? throw new ArgumentNullException(nameof(env));

    protected virtual void AddHttpContextAccessor(IServiceCollection services)
    {
        services
            .AddHttpContextAccessor();
    }

    protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
    {
        b.AddAspNetCoreResourceFactory();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        AddHttpContextAccessor(services);

        services
            // http client factory
            .AddHttpClient()
            // CORS
            .AddCors(b => b.AddDefaultPolicy(opts => opts
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                // must be at least 2 domains for CORS middleware to send Vary: Origin
                .WithOrigins("https://example.com", "http://127.0.0.1")
                .SetIsOriginAllowed(_ => true)
            ));
        base.ConfigureServices(services);
    }

    public virtual void Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
    {
        ConfigureBase(serviceProvider);
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
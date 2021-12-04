using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NCoreUtils.Videos.WebService
{
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
/*
        private static Func<StorageClient> InitializeStorageClient()
        {
            StorageClient? initializedClient = default;
            Task<StorageClient> pendingClient = DoInitializeAsync();
            return () => initializedClient ?? pendingClient.Result;

            async Task<StorageClient> DoInitializeAsync()
            {
                var client = await StorageClient.CreateAsync();
                initializedClient = client;
                return client;
            }
        }
*/

        private readonly IConfiguration Configuration;

        private readonly IWebHostEnvironment _env;

        protected CoreStartup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

#if !NETCOREAPP3_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(VideoResizerOptions))]
#endif
        protected virtual IVideoResizerOptions GetVideoResizerOptions()
            => Configuration.GetSection("Videos")
                .Get<VideoResizerOptions>()
                ?? VideoResizerOptions.Default;

        protected virtual void AddHttpContextAccessor(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor();
        }

        protected abstract void ConfigureResourceFactories(CompositeResourceFactoryBuilder b);

        public virtual void ConfigureServices(IServiceCollection services)
        {
            AddHttpContextAccessor(services);

            services
                // JSON Serialization
                .AddOptions<JsonSerializerOptions>()
                    .Configure(opts => opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                    .Services
                // image resizer options
                .AddSingleton(GetVideoResizerOptions())
                // http client factory
                .AddHttpClient()
                // Video resizer implementation
                .AddXabeVideoResizer()
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
                .AddResourceFactories(ConfigureResourceFactories);
        }

        public virtual void Configure(IApplicationBuilder app)
        {
#if DEBUG
            if (_env.IsDevelopment())
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
}

using System;
using System.Text.Json;
using System.Threading.Tasks;
//using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCoreUtils.AspNetCore;
using NCoreUtils.Images;
//using NCoreUtils.Videos.Internal;

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
        private readonly IConfiguration _configuration;

        private readonly IWebHostEnvironment _env;

        protected CoreStartup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }
        protected virtual void AddHttpContextAccessor(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor();
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            //var storageClientAccessor = InitializeStorageClient();
            AddHttpContextAccessor(services);

            services
                // http client factory
                .AddHttpClient()
                // JSON serialization
                .AddOptions<JsonSerializerOptions>().Services
                .ConfigureOptions<ConfigureJson>()
                .AddTransient(serviceProvider => serviceProvider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().CurrentValue)
                // Google Cloud Storage client
               // .AddTransient(_ => storageClientAccessor())
                // Video resizer implementation
                .AddSingleton<IVideoResizer, VideoResizer>()
                // Image resizer client
                //.AddImageResizerClient(_configuration["Images:Endpoint"], true, true)
                // CORS
                .AddCors(b => b.AddDefaultPolicy(opts => opts
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    // must be at least 2 domains for CORS middleware to send Vary: Origin
                    .WithOrigins("https://example.com", "http://127.0.0.1")
                    .SetIsOriginAllowed(_ => true)
                ))
                // routing
                .AddRouting();
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
#if !DEBUG
                .UsePrePopulateLoggingContext()
#endif
                .UseCors()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    // endpoints.MapProto<IVideoResizer>(b => b.ApplyVideoWebServiceDefaults());
                });
        }
    }
}

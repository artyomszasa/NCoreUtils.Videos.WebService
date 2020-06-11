using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore;
using NCoreUtils.AspNetCore.Proto;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils
{
    public static class ServiceCollectionVideoResizerClientExtensions
    {
        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IEndpointConfiguration configuration, string? path)
            => services.AddProtoClient<IVideoResizer>(configuration, b => b.ApplyVideoWebServiceDefaults(path));

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IConfiguration configuration, string? path)
            => services.AddProtoClient<IVideoResizer>(configuration, b => b.ApplyVideoWebServiceDefaults(path));

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, string endpoint, string? path)
            => services.AddProtoClient<IVideoResizer>(new EndpointConfiguration { Endpoint = endpoint }, b => b.ApplyVideoWebServiceDefaults(path));

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IEndpointConfiguration configuration)
            => services.AddProtoClient<IVideoResizer>(configuration, b => b.ApplyVideoWebServiceDefaults());

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IConfiguration configuration)
            => services.AddProtoClient<IVideoResizer>(configuration, b => b.ApplyVideoWebServiceDefaults());

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, string endpoint)
            => services.AddProtoClient<IVideoResizer>(new EndpointConfiguration { Endpoint = endpoint }, b => b.ApplyVideoWebServiceDefaults());
    }
}
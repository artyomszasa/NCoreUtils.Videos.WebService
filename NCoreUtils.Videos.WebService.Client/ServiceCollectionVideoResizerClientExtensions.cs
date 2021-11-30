//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils
{
    public static class ServiceCollectionVideoResizerClientExtensions
    {
        /*
        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IEndpointConfiguration configuration, string? path)
            => services;

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IConfiguration configuration, string? path)
            => services;

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, string endpoint, string? path)
            => services;

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IEndpointConfiguration configuration)
            => services;

        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, IConfiguration configuration)
            => services;
*/
        public static IServiceCollection AddVideoResizerClient(this IServiceCollection services, string endpoint)
            => services;
    }
}
using System;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Videos
{
    public static class ServiceCollectionResourceFactoryExtensions
    {
        public static IServiceCollection AddResourceFactory<TFactory>(this IServiceCollection services)
            where TFactory : class, IResourceFactory
            => services.AddSingleton<IResourceFactory, TFactory>();

        public static IServiceCollection AddResourceFactories(this IServiceCollection services, Action<CompositeResourceFactoryBuilder> build)
        {
            var builder = new CompositeResourceFactoryBuilder();
            build(builder);
            return services
                .AddSingleton(builder)
                .AddResourceFactory<CompositeResourceFactory>();
        }
    }
}
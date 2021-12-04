using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Videos
{
    public class CompositeResourceFactory : IResourceFactory
    {
        public IReadOnlyList<IResourceFactory> Factories { get; }

        public CompositeResourceFactory(IServiceProvider serviceProvider, CompositeResourceFactoryBuilder builder)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            Factories = builder.Factories
                .Select(type => (IResourceFactory)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
        }

        public IVideoSource CreateSource(Uri? uri, Func<IVideoSource> next)
        {
            return DoCreateSource(0);

            IVideoSource DoCreateSource(int index)
            {
                if (index < Factories.Count)
                {
                    return Factories[index].CreateSource(uri, () => DoCreateSource(index + 1));
                }
                return next();
            }
        }

        public IVideoDestination CreateDestination(Uri? uri, Func<IVideoDestination> next)
        {
            return DoCreateDestination(0);

            IVideoDestination DoCreateDestination(int index)
            {
                if (index < Factories.Count)
                {
                    return Factories[index].CreateDestination(uri, () => DoCreateDestination(index + 1));
                }
                return next();
            }
        }
    }
}
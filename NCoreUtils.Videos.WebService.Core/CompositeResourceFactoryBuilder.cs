using System;
using System.Collections.Generic;

namespace NCoreUtils.Videos
{
    public class CompositeResourceFactoryBuilder
    {
        internal List<Type> Factories { get; } = new List<Type>();

        public CompositeResourceFactoryBuilder Add(Type factoryType)
        {
            if (!typeof(IResourceFactory).IsAssignableFrom(factoryType))
            {
                throw new InvalidOperationException($"Type {factoryType} is not a resource factory.");
            }
            Factories.Add(factoryType);
            return this;
        }

        public CompositeResourceFactoryBuilder Add<TFactory>()
            where TFactory : IResourceFactory
            => Add(typeof(TFactory));
    }
}
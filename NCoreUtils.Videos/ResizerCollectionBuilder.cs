using System.Collections.Generic;
using NCoreUtils.Videos.Internal;

namespace NCoreUtils.Videos;

public class ResizerCollectionBuilder
{
    readonly Dictionary<string, IResizerFactory> _factories = new();

    public ResizerCollectionBuilder Add(string name, IResizerFactory factory)
    {
        _factories.Add(name, factory);
        return this;
    }

    public ResizerCollection Build()
        => new(_factories);
}
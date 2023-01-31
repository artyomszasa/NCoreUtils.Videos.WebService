using System.Collections.Generic;

namespace NCoreUtils.Videos.Internal;

public interface IResizer
{
    IAsyncEnumerable<VideoTransformation> PopulateTransformations(IVideo videoInfo);
}
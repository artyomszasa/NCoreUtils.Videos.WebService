using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal
{
    public interface IVideoProvider
    {
        ValueTask<IVideo> FromStreamAsync(Stream source, CancellationToken cancellationToken = default);
    }
}
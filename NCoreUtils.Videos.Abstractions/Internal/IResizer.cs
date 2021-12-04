using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal
{
    public interface IResizer
    {
        ValueTask ResizeAsync(IVideo videoInfo, CancellationToken cancellationToken = default);
    }
}
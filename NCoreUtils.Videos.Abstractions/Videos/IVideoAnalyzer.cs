using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Videos;

namespace NCoreUtils
{
    public interface IVideoAnalyzer
    {
        Task<VideoInfo> AnalyzeAsync(IVideoSource source, CancellationToken cancellationToken);

    }
}
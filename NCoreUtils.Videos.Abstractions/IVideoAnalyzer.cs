using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos;

public interface IVideoAnalyzer
{
    ValueTask<VideoInfo> AnalyzeAsync(IReadableResource source, CancellationToken cancellationToken = default);
}
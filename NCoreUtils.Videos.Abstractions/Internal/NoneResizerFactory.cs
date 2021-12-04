using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Videos.Internal
{
    public class NoneResizerFactory : IResizerFactory
    {
        sealed class NoneResizer : IResizer
        {
            public static NoneResizer Instance { get; } = new NoneResizer();

            NoneResizer() { }

            public ValueTask ResizeAsync(IVideo image, CancellationToken cancellationToken = default)
                => default;
        }


        public IResizer CreateResizer(IVideo image, ResizeOptions options) => NoneResizer.Instance;
    }
}
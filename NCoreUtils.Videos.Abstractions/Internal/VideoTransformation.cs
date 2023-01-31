using System;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Videos.Internal;

public readonly struct VideoTransformation
{
    private sealed class Tag
    {
        public const int Noop = 0;

        public const int Resize = 1;

        public const int Crop = 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VideoTransformation Resize(Size size)
        => new(Tag.Resize, size, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VideoTransformation Crop(Rectangle rect)
        => new(Tag.Crop, default, rect);

    private readonly int _tag;

    private readonly Size _size;

    private readonly Rectangle _rect;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VideoTransformation(int tag, Size size, Rectangle rect)
    {
        _tag = tag;
        _size = size;
        _rect = rect;
    }

    public TResult Accept<TArg, TResult>(IVideoTransformationVisitor<TArg, TResult> visitor, TArg arg) => _tag switch
    {
        Tag.Noop => visitor.VisitNoop(arg),
        Tag.Resize => visitor.VisitResize(arg, _size),
        Tag.Crop => visitor.VisitCrop(arg, _rect),
        _ => throw new InvalidOperationException($"Should never happen: video transformation tag = {_tag}.")
    };
}
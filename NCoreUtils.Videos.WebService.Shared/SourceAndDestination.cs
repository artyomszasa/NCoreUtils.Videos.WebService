using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NCoreUtils.Videos.WebService;

[JsonConverter(typeof(SourceAndDestinationConverter))]
public struct SourceAndDestination : IEquatable<SourceAndDestination>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(SourceAndDestination a, SourceAndDestination b)
        => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(SourceAndDestination a, SourceAndDestination b)
        => !a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool Eq(Uri? a, Uri? b)
    {
        if (a is null)
        {
            return b is null;
        }
        if (b is null)
        {
            return false;
        }
        return a.Equals(b);
    }

    public Uri? Source { get; }

    public Uri? Destination { get; }

    [JsonConstructor]
    public SourceAndDestination(Uri? source, Uri? destination)
    {
        Source = source;
        Destination = destination;
    }

    public bool Equals(SourceAndDestination other)
        => Eq(Source, other.Source) && Eq(Destination, other.Destination);

    public override bool Equals(object? obj)
        => obj is SourceAndDestination other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Source, Destination);

    public override string ToString()
        => $"[{nameof(Source)} = {Source}, {nameof(Destination)} = {Destination}]";
}
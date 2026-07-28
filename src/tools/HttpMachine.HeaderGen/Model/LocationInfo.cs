using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HttpMachine.HeaderGen.Model;

/// <summary>
/// A value-equatable stand-in for <see cref="Location"/>.
/// </summary>
/// <remarks>
/// <see cref="Location"/> is a reference type without value equality, so putting one in the
/// pipeline breaks caching for the same reason <c>ImmutableArray</c> does. We carry the raw
/// offsets instead and rebuild the real <see cref="Location"/> at the point of reporting, inside
/// <c>RegisterSourceOutput</c>, where caching no longer matters.
/// </remarks>
internal readonly record struct LocationInfo(string FilePath, int Start, int Length, int Line)
{
    public Location ToLocation() => Location.Create(
        FilePath,
        new TextSpan(Start, Length),
        new LinePositionSpan(
            new LinePosition(Line, 0),
            new LinePosition(Line, Length)));
}

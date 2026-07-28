using System.Collections.Generic;

namespace HttpMachine.HeaderGen.Model;

/// <summary>
/// Minimal stand-in for <c>System.HashCode</c>, which does not exist on netstandard2.0.
/// </summary>
/// <remarks>
/// Deliberately a plain FNV-1a style combiner rather than a port of the real thing. Hash codes
/// here never leave the process - they only bucket values inside the generator pipeline's caches
/// - so the randomised seeding and avalanche quality of the framework implementation buy nothing,
/// and a stable, obvious combiner is easier to trust.
/// </remarks>
internal struct HashCode
{
    private const int Seed = unchecked((int)2166136261);
    private const int Prime = 16777619;

    private int _hash;
    private bool _started;

    public void Add<T>(T value)
    {
        if (!_started)
        {
            _hash = Seed;
            _started = true;
        }

        var next = value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(value);

        unchecked
        {
            _hash = (_hash ^ next) * Prime;
        }
    }

    public int ToHashCode() => _started ? _hash : 0;
}

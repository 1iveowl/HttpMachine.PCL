using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using HttpMachine.Headers;

namespace HeaderMatcher.Benchmarks;

/// <summary>
/// Compares the generated matcher against the dictionary lookups it is meant to replace.
/// </summary>
/// <remarks>
/// Results are per single header lookup: <c>OperationsPerInvoke</c> divides by the corpus size, so
/// the numbers can be read directly rather than per 48-name pass.
/// </remarks>
[MemoryDiagnoser]
public class HeaderLookupBenchmarks
{
    private static readonly Dictionary<string, KnownHeader> Dictionary = BuildDictionary();
    private static readonly FrozenDictionary<string, KnownHeader> Frozen =
        BuildDictionary().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, KnownHeader> BuildDictionary()
    {
        var dictionary = new Dictionary<string, KnownHeader>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < KnownHeaders.Names.Length; i++)
        {
            dictionary.Add(KnownHeaders.Names[i], (KnownHeader)i);
        }

        return dictionary;
    }

    [GlobalSetup]
    public void Setup()
    {
        if (HeaderCorpus.Names.Length != HeaderCorpus.Size)
        {
            throw new InvalidOperationException(
                $"HeaderCorpus.Size is {HeaderCorpus.Size} but the corpus holds {HeaderCorpus.Names.Length}. "
                + "OperationsPerInvoke would report the wrong per-lookup cost.");
        }
    }

    /// <summary>What the parser will do: match straight off the wire bytes.</summary>
    [Benchmark(Baseline = true, OperationsPerInvoke = HeaderCorpus.Size, Description = "Generated span matcher")]
    public int GeneratedMatcher()
    {
        var found = 0;

        foreach (var name in HeaderCorpus.Utf8)
        {
            if (KnownHeaders.TryMatch(name, out _, out _, out _))
            {
                found++;
            }
        }

        return found;
    }

    /// <summary>
    /// The honest alternative for byte input: materialise a string, then look it up. This is what
    /// the parser would have to do without a span matcher, and it includes the allocation that
    /// forces.
    /// </summary>
    [Benchmark(OperationsPerInvoke = HeaderCorpus.Size, Description = "GetString + Dictionary (OrdinalIgnoreCase)")]
    public int MaterialiseThenDictionary()
    {
        var found = 0;

        foreach (var name in HeaderCorpus.Utf8)
        {
            if (Dictionary.TryGetValue(Encoding.ASCII.GetString(name), out _))
            {
                found++;
            }
        }

        return found;
    }

    /// <summary>
    /// Pure lookup cost with the string already in hand. The hardest baseline, and not a fair
    /// stand-in for what the parser does - a byte parser never has the string for free. Included
    /// because it isolates how much of any win is avoided hashing versus avoided allocation.
    /// </summary>
    [Benchmark(OperationsPerInvoke = HeaderCorpus.Size, Description = "Dictionary on an existing string")]
    public int DictionaryOnExistingString()
    {
        var found = 0;

        foreach (var name in HeaderCorpus.Names)
        {
            if (Dictionary.TryGetValue(name, out _))
            {
                found++;
            }
        }

        return found;
    }

    /// <summary>
    /// Reference only. FrozenDictionary is .NET 8+, so it cannot ship in a library that targets
    /// netstandard2.0 with one implementation for all frameworks. Measured so the cost of that
    /// constraint is known rather than assumed.
    /// </summary>
    [Benchmark(OperationsPerInvoke = HeaderCorpus.Size, Description = "FrozenDictionary, existing string (not shippable)")]
    public int FrozenDictionaryOnExistingString()
    {
        var found = 0;

        foreach (var name in HeaderCorpus.Names)
        {
            if (Frozen.TryGetValue(name, out _))
            {
                found++;
            }
        }

        return found;
    }
}

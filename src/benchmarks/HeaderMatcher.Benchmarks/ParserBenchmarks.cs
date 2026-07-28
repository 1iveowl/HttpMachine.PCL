using System;
using BenchmarkDotNet.Attributes;
using HttpMachine;

namespace HeaderMatcher.Benchmarks;

/// <summary>
/// End-to-end parse of a whole message in both header-name modes.
/// </summary>
/// <remarks>
/// The "before" half of the comparison lives in <c>tools/benchmark-against-head.sh</c>, which runs
/// the same corpus against a git worktree at HEAD. It cannot be a row in this table: the released
/// and locally built assemblies share the identity HttpMachine 6.1.0.0, so MSBuild's conflict
/// resolution drops one of them however the extern alias is arranged. Two passes on one machine in
/// one session are directly comparable; they just do not get a ratio column.
/// </remarks>
[MemoryDiagnoser]
public class ParserBenchmarks
{
    private byte[] _message = null!;

    private HttpCombinedParser _asReceived = null!;
    private HttpCombinedParser _canonical = null!;
    private HttpCombinedParser _asReceivedReal = null!;
    private HttpCombinedParser _canonicalReal = null!;

    [Params("Request", "Response", "SsdpNotify")]
    public string Message { get; set; } = "Request";

    /// <summary>Whether to drive the ready-made delegate consumers actually use.</summary>
    [Params(false, true)]
    public bool RealDelegate { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _message = Messages.Get(Message);

        _asReceived = new HttpCombinedParser(new NullDelegate(), HeaderNameMode.AsReceived);
        _canonical = new HttpCombinedParser(new NullDelegate(), HeaderNameMode.Canonical);
        _asReceivedReal = new HttpCombinedParser(new HttpParserDelegate(), HeaderNameMode.AsReceived);
        _canonicalReal = new HttpCombinedParser(new HttpParserDelegate(), HeaderNameMode.Canonical);

        // A benchmark of a parser that is quietly failing would just measure how fast it gives up.
        AssertConsumesEverything(_asReceived.Execute(_message), nameof(HeaderNameMode.AsReceived));
        AssertConsumesEverything(_canonical.Execute(_message), nameof(HeaderNameMode.Canonical));
    }

    private void AssertConsumesEverything(int consumed, string which)
    {
        if (consumed != _message.Length)
        {
            throw new InvalidOperationException(
                $"{which} consumed {consumed} of {_message.Length} bytes of the {Message} message.");
        }
    }

    [Benchmark(Baseline = true, Description = "AsReceived (new default)")]
    public int AsReceived() =>
        RealDelegate ? _asReceivedReal.Execute(_message) : _asReceived.Execute(_message);

    [Benchmark(Description = "Canonical (opt-in)")]
    public int Canonical() =>
        RealDelegate ? _canonicalReal.Execute(_message) : _canonical.Execute(_message);
}

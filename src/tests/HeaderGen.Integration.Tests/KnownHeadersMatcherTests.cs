using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HttpMachine.Headers;
using Xunit;

namespace HeaderGen.Integration.Tests;

/// <summary>
/// Executes the generated matcher against a dictionary oracle built from the same header list.
/// </summary>
/// <remarks>
/// The oracle is the point. A hand-written expectation per header would only ever re-state what the
/// generator emitted; comparing against
/// <c>Dictionary&lt;string, KnownHeader&gt;(StringComparer.OrdinalIgnoreCase)</c> checks the thing
/// that actually matters - that all the chunking, overlapping reads and per-position case masks add
/// up to plain case-insensitive string equality.
/// </remarks>
public class KnownHeadersMatcherTests
{
    /// <summary>Every known header, canonical spelling, excluding the None slot.</summary>
    public static readonly string[] AllNames = KnownHeaders.Names.Skip(1).ToArray();

    private static readonly Dictionary<string, KnownHeader> Oracle = BuildOracle();

    private static Dictionary<string, KnownHeader> BuildOracle()
    {
        var oracle = new Dictionary<string, KnownHeader>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < KnownHeaders.Names.Length; i++)
        {
            oracle.Add(KnownHeaders.Names[i], (KnownHeader)i);
        }

        return oracle;
    }

    public static TheoryData<string> KnownNames()
    {
        var data = new TheoryData<string>();

        foreach (var name in AllNames)
        {
            data.Add(name);
        }

        return data;
    }

    private static bool TryMatch(string name, out KnownHeader header, out string canonical, out bool exactCasing) =>
        KnownHeaders.TryMatch(Encoding.UTF8.GetBytes(name), out header, out canonical, out exactCasing);

    // ---------------------------------------------------------------------------------------
    // The generated table matches its source
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Guards the plumbing rather than the matcher: the committed enum and name table must reflect
    /// the header list on disk, in order.
    /// </summary>
    [Fact]
    public void GeneratedNamesMatchTheInputFile()
    {
        var expected = File.ReadAllLines("KnownHeaders.txt")
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && line[0] != '#')
            .Select(line => line.Contains('=') ? line.Split('=')[0].Trim() : line)
            .ToArray();

        Assert.Equal(expected, AllNames);
    }

    /// <summary>
    /// The parser acts on exactly these four. Deleting one from KnownHeaders.txt would silently
    /// break message framing, so it fails here instead.
    /// </summary>
    // The expected member is passed as a string, not a KnownHeader: the enum is internal to the
    // generated code and cannot appear in a public xunit test signature.
    [Theory]
    [InlineData("Content-Length", nameof(KnownHeader.ContentLength))]
    [InlineData("Transfer-Encoding", nameof(KnownHeader.TransferEncoding))]
    [InlineData("Connection", nameof(KnownHeader.Connection))]
    [InlineData("Upgrade", nameof(KnownHeader.Upgrade))]
    public void FramingHeadersArePresent(string name, string expectedMember)
    {
        Assert.True(TryMatch(name, out var header, out var canonical, out _));
        Assert.Equal(expectedMember, header.ToString());
        Assert.Equal(name, canonical);
    }

    // ---------------------------------------------------------------------------------------
    // Matching
    // ---------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(KnownNames))]
    public void MatchesEveryCasingOfEveryKnownHeader(string name)
    {
        foreach (var variant in CasingVariants(name))
        {
            Assert.True(TryMatch(variant, out var header, out var canonical, out _), variant);
            Assert.Equal(Oracle[name], header);
            Assert.Equal(name, canonical);
            Assert.Same(KnownHeaders.Names[(int)header], canonical);
        }
    }

    /// <summary>
    /// The flag the default HeaderNameMode depends on: true only when the wire bytes are
    /// byte-identical to the canonical spelling.
    /// </summary>
    [Theory]
    [MemberData(nameof(KnownNames))]
    public void ExactCasingIsReportedOnlyForTheCanonicalSpelling(string name)
    {
        Assert.True(TryMatch(name, out _, out _, out var canonicalExact));
        Assert.True(canonicalExact, $"'{name}' is the canonical spelling but was not reported as exact");

        foreach (var variant in CasingVariants(name).Where(v => !string.Equals(v, name, StringComparison.Ordinal)))
        {
            Assert.True(TryMatch(variant, out _, out _, out var exact), variant);
            Assert.False(exact, $"'{variant}' differs from canonical '{name}' but was reported as exact");
        }
    }

    /// <summary>
    /// Truncating, extending or mutating a known name must agree with the oracle - which almost
    /// always means "no match", but not always: mutating a letter's case still matches.
    /// </summary>
    [Theory]
    [MemberData(nameof(KnownNames))]
    public void NearMissesAgreeWithTheOracle(string name)
    {
        AssertAgreesWithOracle(name.Substring(0, name.Length - 1));
        AssertAgreesWithOracle(name + "x");
        AssertAgreesWithOracle("x" + name);

        // Every single-position mutation to another token character.
        foreach (var position in Enumerable.Range(0, name.Length))
        {
            foreach (var replacement in "aZ0-.!_")
            {
                var mutated = name.Substring(0, position) + replacement + name.Substring(position + 1);
                AssertAgreesWithOracle(mutated);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("X-Request-Id")]
    [InlineData("A")]
    [InlineData("Content-Lengtx")]
    [InlineData("Cache-Contral")]
    [InlineData("Conten-Length")]
    [InlineData("X-Correlation-Identifier")]
    public void UnknownNamesDoNotMatch(string name)
    {
        Assert.False(TryMatch(name, out var header, out var canonical, out var exact));
        Assert.Equal(KnownHeader.None, header);
        Assert.Equal(string.Empty, canonical);
        Assert.False(exact);
    }

    /// <summary>
    /// A broad sweep for false positives: nothing in the token alphabet should match that the
    /// oracle does not.
    /// </summary>
    /// <remarks>
    /// Worth being honest about how weak this is despite its size. Replacing the per-position case
    /// mask with a naive blanket <c>| 0x20</c> - the classic bug this whole design exists to avoid
    /// - passes all 200,000 probes here, because a random token string essentially never lands
    /// within one byte of a real header name. Only
    /// <see cref="SingleBytePerturbationsOfKnownNamesAgreeWithTheOracle"/> catches that, and it was
    /// confirmed to by deliberately introducing the bug. Keep both; do not mistake this one's
    /// probe count for coverage.
    /// </remarks>
    [Fact]
    public void RandomTokenNamesAgreeWithTheOracle()
    {
        const string TokenAlphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&'*+-.^_`|~";

        var random = new Random(20260728); // fixed seed: a failure must be reproducible
        var builder = new StringBuilder();

        for (var i = 0; i < 200_000; i++)
        {
            builder.Clear();

            var length = random.Next(1, 30);

            for (var c = 0; c < length; c++)
            {
                builder.Append(TokenAlphabet[random.Next(TokenAlphabet.Length)]);
            }

            AssertAgreesWithOracle(builder.ToString());
        }
    }

    /// <summary>
    /// Every single-byte perturbation of every known name, over all 256 byte values.
    /// </summary>
    /// <remarks>
    /// This is the test that actually secures the case-folding design, and the only one that fails
    /// when the per-position mask is replaced by a naive blanket <c>| 0x20</c>. Sweeping all 256
    /// values rather than just token characters is deliberate: the bug it catches is a control byte
    /// folding onto a letter or onto '-', which no token-only probe would ever produce.
    /// </remarks>
    [Fact]
    public void SingleBytePerturbationsOfKnownNamesAgreeWithTheOracle()
    {
        foreach (var name in AllNames)
        {
            var bytes = Encoding.UTF8.GetBytes(name);

            for (var position = 0; position < bytes.Length; position++)
            {
                var original = bytes[position];

                // Every byte value, not just token characters: a matcher must not be fooled by
                // control bytes that happen to fold onto a letter or a '-'.
                for (var value = 0; value < 256; value++)
                {
                    bytes[position] = (byte)value;

                    var matched = KnownHeaders.TryMatch(bytes, out var header, out _, out _);
                    var expected = Oracle.TryGetValue(Encoding.Latin1.GetString(bytes), out var oracleHeader);

                    Assert.Equal(expected, matched);

                    if (expected)
                    {
                        Assert.Equal(oracleHeader, header);
                    }
                }

                bytes[position] = original;
            }
        }
    }

    private static void AssertAgreesWithOracle(string probe)
    {
        var matched = TryMatch(probe, out var header, out var canonical, out var exact);
        var expected = Oracle.TryGetValue(probe, out var oracleHeader);

        Assert.Equal(expected, matched);

        if (!expected)
        {
            return;
        }

        Assert.Equal(oracleHeader, header);
        Assert.Equal(KnownHeaders.Names[(int)oracleHeader], canonical);
        Assert.Equal(string.Equals(probe, canonical, StringComparison.Ordinal), exact);
    }

    private static IEnumerable<string> CasingVariants(string name)
    {
        yield return name;
        yield return name.ToUpperInvariant();
        yield return name.ToLowerInvariant();
        yield return Alternate(name);

        static string Alternate(string value)
        {
            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = i % 2 == 0 ? char.ToUpperInvariant(chars[i]) : char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }
    }
}

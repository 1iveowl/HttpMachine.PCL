using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using static VerifyXunit.Verifier;

namespace HeaderGen.Tests;

/// <summary>
/// Snapshot tests over the generated matcher.
/// </summary>
/// <remarks>
/// Each case uses a deliberately tiny header list chosen to exercise one structural feature of the
/// emitter. Snapshotting the real 73-header list would produce an 1800-line file that nobody would
/// read a diff of; the properties of the real list are covered by the integration tests and the CI
/// drift check instead.
/// </remarks>
public class KnownHeaderMatcherGeneratorSnapshotTests
{
    /// <summary>One 2-byte name: a single ushort read, no overlap, no first-byte switch.</summary>
    [Fact]
    public Task SingleShortHeader() => VerifyList("TE");

    /// <summary>Exactly 8 bytes: one ulong read that covers the name with nothing left over.</summary>
    [Fact]
    public Task NameOfExactlyEightBytes() => VerifyList("If-Match");

    /// <summary>
    /// Nine bytes: the tail is covered by a second, overlapping 8-byte read rather than by
    /// stepping down through a uint, a ushort and a byte.
    /// </summary>
    [Fact]
    public Task NameRequiringAnOverlappingRead() => VerifyList("Forwarded");

    /// <summary>A single byte: the one case with no BinaryPrimitives read at all.</summary>
    [Fact]
    public Task SingleByteName() => VerifyList("A");

    /// <summary>
    /// Several names of the same length, two of which share a first byte. Exercises the two-level
    /// dispatch: switch on length, then on the folded first byte, then compare.
    /// </summary>
    [Fact]
    public Task SeveralNamesOfTheSameLength() => VerifyList(
        """
        Cache-Control
        Content-Range
        Authorization
        Last-Modified
        """);

    /// <summary>
    /// Digits and '.' are token characters but not letters, so the case-fold mask must be zero at
    /// those positions. This is exactly what a blanket "| 0x20" would get wrong.
    /// </summary>
    [Fact]
    public Task NameWithNonLetterPositions() => VerifyList("BOOTID.UPNP.ORG = BootIdUpnpOrg");

    /// <summary>A name with no letters at all needs no folded comparison, only the exact one.</summary>
    [Fact]
    public Task NameWithNoLetters() => VerifyList("123 = Numeric");

    [Fact]
    public Task EnumMemberOverride() => VerifyList("X-Forwarded-For = ForwardedFor");

    [Fact]
    public Task CommentsAndBlankLinesAreIgnored() => VerifyList(
        """
        # a comment

        Host

        # another
        """);

    [Fact]
    public Task EmptyHeaderList() => VerifyList("# nothing here");

    [Fact]
    public Task InvalidTokenIsRejected() => VerifyList("Content Length");

    [Fact]
    public Task DuplicateNameIsRejected() => VerifyList("Host\nHOST");

    [Fact]
    public Task DuplicateEnumMemberIsRejected() => VerifyList("Content-Length\nContentLength");

    [Fact]
    public Task InvalidEnumMemberIsRejected() => VerifyList("Host = 1Host");

    [Fact]
    public Task MalformedEntryIsRejected() => VerifyList("Host = A = B");

    [Fact]
    public Task MissingInputFile() => VerifyFiles([new InMemoryAdditionalText("Unrelated.txt", "Host")]);

    /// <summary>The two paths are listed out of order to prove the diagnostic message is sorted.</summary>
    [Fact]
    public Task DuplicateInputFile() => VerifyFiles([
        new InMemoryAdditionalText("/b/KnownHeaders.txt", "Host"),
        new InMemoryAdditionalText("/a/KnownHeaders.txt", "Host")]);

    /// <summary>
    /// A snapshot can only be trusted if the code it captures actually compiles. Approving output
    /// that does not build is the failure mode snapshot testing invites, so every valid list is
    /// compiled as well as snapshotted.
    /// </summary>
    [Theory]
    [InlineData("TE")]
    [InlineData("A")]
    [InlineData("If-Match")]
    [InlineData("Forwarded")]
    [InlineData("123 = Numeric")]
    [InlineData("BOOTID.UPNP.ORG = BootIdUpnpOrg")]
    [InlineData("Strict-Transport-Security")]
    [InlineData("# nothing here")]
    public void GeneratedCodeCompiles(string headerList)
    {
        var errors = TestHelper.RunAndCompile(headerList)
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToList();

        Assert.Empty(errors);
    }

    private static Task VerifyList(string headerList, [CallerMemberName] string testName = "") =>
        Verify(TestHelper.Run(headerList))
            .UseDirectory("Snapshots")
            .UseMethodName(testName);

    // Not "params": that would have to come last, displacing the CallerMemberName parameter and
    // silently naming every snapshot after this helper instead of the test.
    private static Task VerifyFiles(
        InMemoryAdditionalText[] files,
        [CallerMemberName] string testName = "") =>
        Verify(TestHelper.Run(files))
            .UseDirectory("Snapshots")
            .UseMethodName(testName);
}

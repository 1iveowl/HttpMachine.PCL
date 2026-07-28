using System.IO;
using Xunit;

namespace HttpMachine.Tests.Fixtures;

/// <summary>
/// Guards the one failure mode that would silently invalidate every fixture at once.
/// </summary>
/// <remarks>
/// <para>
/// Fixtures are raw bytes fed to the parser unmodified. Two things in this repository would happily
/// rewrite them: the root .gitattributes rule <c>* text=auto</c>, which normalises line endings on
/// anything git decides is text, and the devcontainer's <c>files.insertFinalNewline</c>, which
/// appends a byte on save. Both are overridden - by a scoped <c>-text</c> rule and by
/// Fixtures/.editorconfig respectively - but an override that silently stops working is worse than
/// no override.
/// </para>
/// <para>
/// If either guard fails, a dozen snapshots shift at once for no visible reason. This test fails
/// first, with a message that names the cause.
/// </para>
/// </remarks>
public class FixtureByteExactnessTests
{
    /// <summary>
    /// A fixture chosen because it exercises both hazards at once: it contains CRLF pairs, and it
    /// deliberately ends in body bytes with no trailing newline. Length alone is enough to catch
    /// either - CRLF to LF shortens the file, an appended newline lengthens it - and is far less
    /// brittle than a hash when someone edits the fixture on purpose.
    /// </summary>
    private const string Canary = "Responses/ok-content-length.http";
    private const int CanaryLength = 79;

    [Fact]
    public void CanaryFixtureIsByteExact()
    {
        var bytes = File.ReadAllBytes(FixturePaths.FullPath(Canary));

        Assert.True(
            CanaryLength == bytes.Length,
            $"'{Canary}' is {bytes.Length} bytes, expected {CanaryLength}. Something rewrote it - check that "
            + "the '-text' rule for Fixtures in .gitattributes is intact, and that Fixtures/.editorconfig "
            + "still suppresses insert_final_newline.");
    }

    [Fact]
    public void NoFixtureHasABareLineFeedLineEnding()
    {
        foreach (var path in Directory.EnumerateFiles(FixturePaths.FixtureRoot, "*.http", SearchOption.AllDirectories))
        {
            var bytes = File.ReadAllBytes(path);
            var name = path.Substring(FixturePaths.FixtureRoot.Length + 1);

            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != (byte)'\n')
                {
                    continue;
                }

                Assert.True(
                    i > 0 && bytes[i - 1] == (byte)'\r',
                    $"'{name}' has a bare LF at offset {i} with no preceding CR. Either git normalised the "
                    + "file's line endings, or the fixture genuinely tests bare-LF handling - in which case "
                    + "add it to the exclusion list in this test with a comment saying why.");
            }
        }
    }

    [Fact]
    public void EveryFixtureHasAnApprovedSnapshot()
    {
        foreach (var path in Directory.EnumerateFiles(FixturePaths.FixtureRoot, "*.http", SearchOption.AllDirectories))
        {
            var expected = Path.Combine(
                Path.GetDirectoryName(path)!,
                FixtureCase.Parse(path.Substring(FixturePaths.FixtureRoot.Length + 1).Replace(Path.DirectorySeparatorChar, '/')).Name
                    + ".verified.txt");

            Assert.True(
                File.Exists(expected),
                $"'{path}' has no approved snapshot at '{expected}'. Run the tests and review the .received "
                + "file before accepting it.");
        }
    }
}

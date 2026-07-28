using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpMachine.Tests.Fixtures;
using Xunit;
using static VerifyXunit.Verifier;

namespace HttpMachine.Tests;

/// <summary>
/// One test case per file in Fixtures/, feeding the raw bytes to the parser and snapshotting the
/// full sequence of delegate callbacks.
/// </summary>
/// <remarks>
/// <para>
/// Adding a test case means dropping a file into Fixtures/Requests or Fixtures/Responses and
/// approving the snapshot. Nothing else is edited.
/// </para>
/// <para>
/// These cover "what does this message parse to". Tests about <em>how</em> the parser is fed - split
/// buffers, span versus array versus MemoryStream, pipelining, when end of stream is signalled - stay
/// hand-written in <see cref="HttpCombinedParserTests"/>, because there the feeding is the subject
/// rather than an incidental.
/// </para>
/// </remarks>
public class FixtureTests
{
    public static TheoryData<string> Fixtures()
    {
        var data = new TheoryData<string>();

        // Sorted, and '/' separated regardless of platform, so the test list is stable.
        var paths = Directory
            .EnumerateFiles(FixturePaths.FixtureRoot, "*.http", SearchOption.AllDirectories)
            .Select(path => path.Substring(FixturePaths.FixtureRoot.Length + 1).Replace(Path.DirectorySeparatorChar, '/'))
            .OrderBy(path => path, StringComparer.Ordinal);

        foreach (var path in paths)
        {
            data.Add(path);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Fixtures))]
    public async Task Fixture(string relativePath)
    {
        var fixture = FixtureCase.Parse(relativePath);
        var bytes = File.ReadAllBytes(FixturePaths.FullPath(relativePath));

        var recorder = new RecordingParserDelegate();
        using var parser = new HttpCombinedParser(recorder, fixture.UnframedResponseMode, fixture.HeaderNameMode);

        Feed(parser, recorder, bytes);

        if (fixture.SignalsEndOfStream)
        {
            Feed(parser, recorder, Array.Empty<byte>(), " (end of stream)");
        }

        recorder.RecordParserState(parser);

        // The snapshot records what happened; these record what was meant. Without them, a change
        // that inverted a fixture's purpose would show up only as a snapshot diff, which is easy to
        // approve without noticing.
        AssertCategoryMatches(fixture, recorder);
        AssertErrorExpectationMatches(fixture, recorder);

        await Verify(recorder.Log)
            .UseDirectory(FixturePaths.SnapshotDirectory(fixture.Category))
            .UseFileName(fixture.Name);
    }

    private static void Feed(HttpCombinedParser parser, RecordingParserDelegate recorder, byte[] bytes, string note = "")
    {
        recorder.BeginExecute(bytes.Length, note);

        try
        {
            recorder.EndExecute(parser.Execute(bytes));
        }
        catch (Exception exception)
        {
            // Captured rather than thrown: a fixture that provokes an unhandled exception should be
            // a reviewable snapshot, not a red test with no detail about where it got to.
            recorder.RecordException(exception);
        }
    }

    private static void AssertCategoryMatches(FixtureCase fixture, RecordingParserDelegate recorder)
    {
        if (fixture.ExpectsParserError && !recorder.SawRequest && !recorder.SawResponse)
        {
            // Input too malformed to classify. That is the fixture's whole point.
            return;
        }

        if (fixture.ExpectsRequest)
        {
            Assert.True(recorder.SawRequest, $"'{fixture.RelativePath}' is under Requests/ but did not parse as a request.");
            Assert.False(recorder.SawResponse, $"'{fixture.RelativePath}' is under Requests/ but parsed as a response.");
        }

        if (fixture.ExpectsResponse)
        {
            Assert.True(recorder.SawResponse, $"'{fixture.RelativePath}' is under Responses/ but did not parse as a response.");
            Assert.False(recorder.SawRequest, $"'{fixture.RelativePath}' is under Responses/ but parsed as a request.");
        }
    }

    /// <summary>
    /// Asserts that a fixture is accepted or rejected as its name says.
    /// </summary>
    /// <remarks>
    /// "Rejected" has two shapes in this parser and a fixture may be either. Input the grammar does
    /// not accept simply stops the state machine, and <c>Execute</c> returns a short count with no
    /// callback at all. <c>OnParserError</c> is raised only when an exception escapes a parser
    /// action, such as a second <c>Content-Length</c>. Checking only the second would have let a
    /// grammar-rejected fixture look like a passing one.
    /// </remarks>
    private static void AssertErrorExpectationMatches(FixtureCase fixture, RecordingParserDelegate recorder)
    {
        var rejected = recorder.SawParserError || !recorder.AcceptedAllInput;

        if (fixture.ExpectsParserError)
        {
            Assert.True(
                rejected,
                $"'{fixture.RelativePath}' is marked .invalid but was parsed in full without an error. "
                + "Either the fixture is no longer malformed, or the parser has become more permissive.");
        }
        else
        {
            Assert.False(
                rejected,
                $"'{fixture.RelativePath}' was rejected by the parser"
                + (recorder.SawParserError ? " (OnParserError was raised)" : " (Execute stopped short of the end)")
                + $". If that is intended, rename it to '{fixture.Name}.invalid.http'.");
        }
    }
}

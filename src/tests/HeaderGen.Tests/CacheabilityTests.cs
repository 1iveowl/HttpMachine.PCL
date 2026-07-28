using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HttpMachine.HeaderGen;
using Microsoft.CodeAnalysis;
using Xunit;

namespace HeaderGen.Tests;

/// <summary>
/// Asserts the generator's pipeline outputs are cacheable.
/// </summary>
/// <remarks>
/// <para>
/// The IDE re-runs generators on every keystroke. A generator whose pipeline values do not compare
/// by value still produces correct output, so nothing visibly breaks - it just re-does all of its
/// work every time, and the only symptom is a sluggish editor. That makes this the one property of
/// an incremental generator that has to be asserted rather than reviewed.
/// </para>
/// <para>
/// Following Andrew Lock, "Creating a source generator - Part 10: Testing your incremental
/// generator pipeline outputs are cacheable".
/// </para>
/// </remarks>
public class CacheabilityTests
{
    private const string HeaderList =
        """
        Content-Length
        Transfer-Encoding
        Connection
        Upgrade
        """;

    /// <summary>
    /// The second run over an identical compilation must reuse everything.
    /// </summary>
    [Fact]
    public void RunningTwiceOverTheSameCompilationReusesEveryStage()
    {
        var (first, second) = RunTwice(static compilation => compilation.Clone());

        AssertAllStagesCached(second);
        AssertStageOutputsAreEqual(first, second);
    }

    /// <summary>
    /// Editing unrelated C# must not disturb the generator.
    /// </summary>
    /// <remarks>
    /// This is the case that would fail if anyone reached for <c>CompilationProvider</c> or
    /// <c>AnalyzerConfigOptionsProvider</c> later on. The generator's only input is a text file, so
    /// changing the compilation around it has to be invisible.
    /// </remarks>
    [Fact]
    public void EditingUnrelatedSourceDoesNotInvalidateTheModel()
    {
        var (first, second) = RunTwice(static compilation => compilation.AddSyntaxTrees(
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("internal class SomethingElse { }")));

        AssertAllStagesCached(second);
        AssertStageOutputsAreEqual(first, second);
    }

    /// <summary>
    /// Changing the header list must actually re-run the pipeline. Without this, a test suite
    /// asserting only "everything was cached" would still pass if the generator were wired to
    /// nothing at all.
    /// </summary>
    [Fact]
    public void ChangingTheHeaderListRebuildsTheModel()
    {
        var compilation = TestHelper.CreateCompilation();

        var firstRun = TestHelper
            .CreateDriver([new InMemoryAdditionalText("KnownHeaders.txt", HeaderList)], trackIncrementalSteps: true)
            .RunGenerators(compilation)
            .GetRunResult();

        var secondRun = TestHelper
            .CreateDriver([new InMemoryAdditionalText("KnownHeaders.txt", HeaderList + "\nHost")], trackIncrementalSteps: true)
            .RunGenerators(compilation)
            .GetRunResult();

        Assert.NotEqual(
            StageOutputs(firstRun, TrackingNames.BuildModel),
            StageOutputs(secondRun, TrackingNames.BuildModel));
    }

    /// <summary>
    /// Nothing reachable from a value model may be a Roslyn object.
    /// </summary>
    /// <remarks>
    /// These types have reference equality, so a single one anywhere in the object graph defeats
    /// the caching the other tests assert - and they also pin large object graphs in memory for as
    /// long as the driver caches them. The file-selection stage is exempt because its element type
    /// is Roslyn's own; see <see cref="TrackingNames.ValueModelStages"/>.
    /// </remarks>
    [Fact]
    public void NoValueModelHoldsARoslynObject()
    {
        var (first, _) = RunTwice(static compilation => compilation.Clone());

        foreach (var name in TrackingNames.ValueModelStages)
        {
            foreach (var value in StageOutputs(first, name))
            {
                AssertObjectGraphIsSerialisable(value, name);
            }
        }
    }

    private static (GeneratorDriverRunResult First, GeneratorDriverRunResult Second) RunTwice(
        Func<Compilation, Compilation> mutate)
    {
        var compilation = TestHelper.CreateCompilation();

        // The same AdditionalText instances are handed to both runs, exactly as a host that has
        // not seen the file change would do.
        var additionalTexts = new[] { new InMemoryAdditionalText("KnownHeaders.txt", HeaderList) };
        var driver = TestHelper.CreateDriver(additionalTexts, trackIncrementalSteps: true);

        driver = driver.RunGenerators(compilation);
        var first = driver.GetRunResult();

        var second = driver.RunGenerators(mutate(compilation)).GetRunResult();

        return (first, second);
    }

    private static void AssertAllStagesCached(GeneratorDriverRunResult result)
    {
        foreach (var name in TrackingNames.All)
        {
            var steps = TrackedSteps(result, name);

            Assert.True(steps.Count > 0, $"No tracked step named '{name}' ran. Has WithTrackingName been removed?");

            foreach (var output in steps.SelectMany(step => step.Outputs))
            {
                Assert.True(
                    output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    $"Stage '{name}' reported {output.Reason}; expected Cached or Unchanged.");
            }
        }
    }

    private static void AssertStageOutputsAreEqual(GeneratorDriverRunResult first, GeneratorDriverRunResult second)
    {
        foreach (var name in TrackingNames.All)
        {
            Assert.Equal(StageOutputs(first, name), StageOutputs(second, name));
        }
    }

    private static IReadOnlyList<IncrementalGeneratorRunStep> TrackedSteps(GeneratorDriverRunResult result, string name) =>
        result.Results
            .SelectMany(generator => generator.TrackedSteps)
            .Where(step => step.Key == name)
            .SelectMany(step => step.Value)
            .ToList();

    private static List<object> StageOutputs(GeneratorDriverRunResult result, string name) =>
        TrackedSteps(result, name)
            .SelectMany(step => step.Outputs)
            .Select(output => output.Value)
            .ToList();

    private static void AssertObjectGraphIsSerialisable(object? value, string stageName)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        Visit(value);

        void Visit(object? node)
        {
            if (node is null || !visited.Add(node))
            {
                return;
            }

            Assert.False(node is Compilation, $"Stage '{stageName}' holds a Compilation.");
            Assert.False(node is ISymbol, $"Stage '{stageName}' holds an ISymbol.");
            Assert.False(node is SyntaxNode, $"Stage '{stageName}' holds a SyntaxNode.");
            Assert.False(node is AdditionalText, $"Stage '{stageName}' holds an AdditionalText.");
            Assert.False(node is Microsoft.CodeAnalysis.Text.SourceText, $"Stage '{stageName}' holds a SourceText.");

            var type = node.GetType();

            if (type.IsPrimitive || node is string || type.IsEnum)
            {
                return;
            }

            if (node is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    Visit(item);
                }

                return;
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Visit(field.GetValue(node));
            }
        }
    }
}

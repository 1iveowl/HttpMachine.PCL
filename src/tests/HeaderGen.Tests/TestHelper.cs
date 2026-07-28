using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using HttpMachine.HeaderGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace HeaderGen.Tests;

/// <summary>Hosts the generator in a <see cref="CSharpGeneratorDriver"/> for the tests.</summary>
internal static class TestHelper
{
    /// <summary>
    /// The metadata references the emitted matcher needs to compile: ReadOnlySpan and
    /// BinaryPrimitives. Without these, the generated file appears to fail for reasons that have
    /// nothing to do with the generator.
    /// </summary>
    private static readonly ImmutableArray<MetadataReference> References = AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
        .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
        .Concat(new[] { (MetadataReference)MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location) })
        .Distinct()
        .ToImmutableArray();

    public static GeneratorDriverRunResult Run(string headerListText, string fileName = "KnownHeaders.txt") =>
        Run(new[] { new InMemoryAdditionalText(fileName, headerListText) });

    public static GeneratorDriverRunResult Run(IEnumerable<AdditionalText> additionalTexts)
    {
        var driver = CreateDriver(additionalTexts);

        return driver.RunGenerators(CreateCompilation()).GetRunResult();
    }

    /// <summary>
    /// Runs the generator and compiles the result.
    /// </summary>
    /// <remarks>
    /// Snapshot tests alone would happily approve output that does not compile. Since this
    /// generator's output is committed and built into a shipping assembly, every snapshot test
    /// also asserts the updated compilation is clean.
    /// </remarks>
    public static ImmutableArray<Diagnostic> RunAndCompile(string headerListText)
    {
        var driver = CreateDriver(new[] { new InMemoryAdditionalText("KnownHeaders.txt", headerListText) });

        driver.RunGeneratorsAndUpdateCompilation(CreateCompilation(), out var updated, out _);

        return updated.GetDiagnostics();
    }

    public static GeneratorDriver CreateDriver(
        IEnumerable<AdditionalText> additionalTexts,
        bool trackIncrementalSteps = false)
    {
        var options = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: trackIncrementalSteps);

        return CSharpGeneratorDriver.Create(
            generators: new[] { new KnownHeaderMatcherGenerator().AsSourceGenerator() },
            additionalTexts: additionalTexts,
            driverOptions: options);
    }

    public static CSharpCompilation CreateCompilation(string source = "") =>
        CSharpCompilation.Create(
            "HeaderGen.Tests.Generated",
            new[] { CSharpSyntaxTree.ParseText(source) },
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}

/// <summary>An <see cref="AdditionalText"/> backed by a string rather than a file on disk.</summary>
internal sealed class InMemoryAdditionalText : AdditionalText
{
    private readonly SourceText _text;

    public InMemoryAdditionalText(string path, string text)
    {
        Path = path;

        // Materialised once. The driver caches on the AdditionalText instance rather than on the
        // SourceText, so this is not what makes the cacheability test pass - but returning a fresh
        // SourceText per call would be a lie about how a real host behaves.
        _text = SourceText.From(text);
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
}

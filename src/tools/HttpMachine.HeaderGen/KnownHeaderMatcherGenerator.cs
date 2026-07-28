using System;
using HttpMachine.HeaderGen.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HttpMachine.HeaderGen;

/// <summary>
/// Generates the known-HTTP-header matcher from headers/KnownHeaders.txt.
/// </summary>
/// <remarks>
/// <para>
/// This is an author-time tool. It runs in this repository's build only, its output is committed to
/// source control, and it is never packed into a NuGet package - the same arrangement the
/// Ragel-generated parser uses. Consumers of HttpMachine gain no analyzer, no dependency and no SDK
/// floor from it.
/// </para>
/// <para>
/// The pipeline deliberately touches neither <c>CompilationProvider</c> nor
/// <c>AnalyzerConfigOptionsProvider</c>. Its only input is the text file, so the generated output
/// depends on nothing else in the compilation - which is what the cacheability test asserts.
/// </para>
/// </remarks>
[Generator]
public sealed class KnownHeaderMatcherGenerator : IIncrementalGenerator
{
    internal const string InputFileName = "KnownHeaders.txt";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Matching on the file name keeps AnalyzerConfigOptionsProvider out of the pipeline. The
        // MSBuild-metadata alternative (CompilerVisibleItemMetadata plus
        // build_metadata.AdditionalFiles.*) would make this stage depend on every .editorconfig and
        // property in the build, for no gain when there is exactly one input file with a fixed name.
        var files = context.AdditionalTextsProvider
            .Where(static file => IsInputFile(file.Path))
            .WithTrackingName(TrackingNames.SelectFile);

        // First hop out of Roslyn's object model: everything downstream is strings and ints, so it
        // compares by value and the driver can cache it.
        var parsed = files
            .Select(static (file, cancellationToken) => new ParsedFile(
                file.Path,
                HeaderListParser.Parse(
                    file.Path,
                    file.GetText(cancellationToken) ?? SourceText.From(string.Empty),
                    cancellationToken)))
            .WithTrackingName(TrackingNames.ParseFile);

        // Collect() yields an ImmutableArray, whose equality is by reference, so this node may
        // report Modified even when nothing changed. The Select immediately below returns a
        // value-equatable HeaderTable and settles back to Unchanged, which is why the cacheability
        // test asserts on BuildModel rather than on the raw collect.
        var model = parsed
            .Collect()
            .Select(static (files, cancellationToken) => HeaderListParser.Merge(files, cancellationToken))
            .WithTrackingName(TrackingNames.BuildModel);

        context.RegisterSourceOutput(model, static (context, table) => Execute(context, table));
    }

    private static void Execute(SourceProductionContext context, HeaderTable table)
    {
        foreach (var diagnostic in table.Diagnostics)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.Get(diagnostic.Id),
                diagnostic.Location?.ToLocation(),
                diagnostic.MessageArgument));
        }

        // Emitted even when a diagnostic was reported. An error already fails the build, and
        // emitting the entries that did validate keeps the failure to one clear message instead of
        // burying it under CS0103s from a missing type.
        context.AddSource(Emitter.HintName, SourceText.From(Emitter.Emit(table), System.Text.Encoding.UTF8));
    }

    /// <summary>
    /// Compares the final path segment without <c>System.IO.Path</c>, so the result does not depend
    /// on which platform the build is running on.
    /// </summary>
    private static bool IsInputFile(string path)
    {
        var start = 0;

        for (var i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] == '/' || path[i] == '\\')
            {
                start = i + 1;
                break;
            }
        }

        return string.Compare(path, start, InputFileName, 0, InputFileName.Length, StringComparison.OrdinalIgnoreCase) == 0
            && path.Length - start == InputFileName.Length;
    }
}

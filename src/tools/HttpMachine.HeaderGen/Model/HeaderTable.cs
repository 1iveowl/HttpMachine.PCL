namespace HttpMachine.HeaderGen.Model;

/// <summary>
/// The complete, validated result of reading KnownHeaders.txt - the single value that the whole
/// pipeline reduces to, and the one whose equality decides whether the emitter re-runs.
/// </summary>
/// <remarks>
/// Everything reachable from here is a string, an int or an <see cref="EquatableArray{T}"/> of
/// those. No <c>AdditionalText</c>, no <c>SourceText</c>, no <c>Compilation</c>, no syntax or
/// symbol. That property is asserted directly by the cacheability test rather than left to
/// reviewer discipline.
/// </remarks>
internal readonly record struct HeaderTable(
    EquatableArray<HeaderSpec> Headers,
    EquatableArray<DiagnosticInfo> Diagnostics)
{
    public static HeaderTable Empty { get; } =
        new(EquatableArray<HeaderSpec>.Empty, EquatableArray<DiagnosticInfo>.Empty);

    public HeaderTable WithDiagnostic(DiagnosticInfo diagnostic) =>
        new(Headers, new EquatableArray<DiagnosticInfo>(Diagnostics.AsImmutableArray().Add(diagnostic)));
}

namespace HttpMachine.HeaderGen.Model;

/// <summary>One parsed KnownHeaders.txt, paired with the path it came from.</summary>
/// <remarks>
/// The path is carried as a plain string rather than keeping the <c>AdditionalText</c> around,
/// so nothing downstream of the first pipeline stage can touch a non-equatable Roslyn type.
/// </remarks>
internal readonly record struct ParsedFile(string Path, HeaderTable Table);

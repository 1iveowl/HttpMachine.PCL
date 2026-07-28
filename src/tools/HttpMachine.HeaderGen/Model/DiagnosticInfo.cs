namespace HttpMachine.HeaderGen.Model;

/// <summary>
/// A diagnostic to be reported, in value-equatable form.
/// </summary>
/// <remarks>
/// <see cref="Microsoft.CodeAnalysis.Diagnostic"/> itself never enters the pipeline - see
/// <see cref="LocationInfo"/> for why. <see cref="Location"/> is null for file-level problems
/// that have no position, such as the input file being absent entirely.
/// </remarks>
internal readonly record struct DiagnosticInfo(string Id, string MessageArgument, LocationInfo? Location);

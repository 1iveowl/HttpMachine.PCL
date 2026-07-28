using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace HttpMachine.HeaderGen;

/// <summary>Diagnostics reported by the known-header matcher generator.</summary>
internal static class Diagnostics
{
    private const string Category = "HttpMachine.HeaderGen";

    public static readonly DiagnosticDescriptor MissingInputFile = new(
        "HHG001",
        "KnownHeaders.txt was not found",
        "No KnownHeaders.txt was supplied as an AdditionalFile, so no header matcher can be generated",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateInputFile = new(
        "HHG002",
        "More than one KnownHeaders.txt was supplied",
        "More than one KnownHeaders.txt was supplied as an AdditionalFile ('{0}'); exactly one is expected",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidToken = new(
        "HHG003",
        "Header name is not a valid token",
        "Header name '{0}' is not a valid RFC 9110 token; the generated matcher's case folding is only "
            + "exact over the token byte set, so non-token names cannot be matched correctly",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateHeaderName = new(
        "HHG004",
        "Duplicate header name",
        "Header name '{0}' is listed more than once; header names are matched case-insensitively, so "
            + "the spellings collide",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateEnumMember = new(
        "HHG005",
        "Duplicate enum member name",
        "Two headers produce the same KnownHeader member name '{0}'; use the 'Name = EnumMember' form "
            + "to disambiguate",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidEnumMember = new(
        "HHG006",
        "Enum member name is not a valid C# identifier",
        "'{0}' is not a valid C# identifier and cannot be used as a KnownHeader member name",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedEntry = new(
        "HHG007",
        "Malformed entry",
        "'{0}' is not a valid entry; expected 'Header-Name' or 'Header-Name = EnumMember'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly Dictionary<string, DiagnosticDescriptor> ById = new()
    {
        [MissingInputFile.Id] = MissingInputFile,
        [DuplicateInputFile.Id] = DuplicateInputFile,
        [InvalidToken.Id] = InvalidToken,
        [DuplicateHeaderName.Id] = DuplicateHeaderName,
        [DuplicateEnumMember.Id] = DuplicateEnumMember,
        [InvalidEnumMember.Id] = InvalidEnumMember,
        [MalformedEntry.Id] = MalformedEntry,
    };

    public static DiagnosticDescriptor Get(string id) => ById[id];
}

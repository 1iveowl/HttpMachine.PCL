using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using HttpMachine.HeaderGen.Model;
using Microsoft.CodeAnalysis.Text;

namespace HttpMachine.HeaderGen;

/// <summary>Turns the text of KnownHeaders.txt into a validated <see cref="HeaderTable"/>.</summary>
internal static class HeaderListParser
{
    /// <summary>The RFC 9110 token character set. Header names outside this set are rejected.</summary>
    private const string TokenPunctuation = "!#$%&'*+-.^_`|~";

    public static HeaderTable Parse(string filePath, SourceText text, CancellationToken cancellationToken)
    {
        var headers = ImmutableArray.CreateBuilder<HeaderSpec>();
        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        // Case-insensitive, because that is how the generated matcher compares names: two entries
        // differing only in case would produce two branches that can never both be reached.
        var seenNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var seenMembers = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in text.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var raw = line.ToString();
            var trimmed = raw.Trim();

            if (trimmed.Length == 0 || trimmed[0] == '#')
            {
                continue;
            }

            var location = new LocationInfo(filePath, line.Span.Start, line.Span.Length, line.LineNumber);

            var separator = trimmed.IndexOf('=');
            string name;
            string member;

            if (separator < 0)
            {
                name = trimmed;
                member = DeriveEnumMember(name);
            }
            else
            {
                name = trimmed.Substring(0, separator).Trim();
                member = trimmed.Substring(separator + 1).Trim();

                if (name.Length == 0 || member.Length == 0 || member.IndexOf('=') >= 0)
                {
                    diagnostics.Add(new DiagnosticInfo("HHG007", trimmed, location));
                    continue;
                }
            }

            if (!IsToken(name))
            {
                diagnostics.Add(new DiagnosticInfo("HHG003", name, location));
                continue;
            }

            if (!IsValidIdentifier(member))
            {
                diagnostics.Add(new DiagnosticInfo("HHG006", member, location));
                continue;
            }

            if (seenNames.ContainsKey(name))
            {
                diagnostics.Add(new DiagnosticInfo("HHG004", name, location));
                continue;
            }

            if (seenMembers.ContainsKey(member))
            {
                diagnostics.Add(new DiagnosticInfo("HHG005", member, location));
                continue;
            }

            seenNames.Add(name, name);
            seenMembers.Add(member, member);
            headers.Add(new HeaderSpec(name, member));
        }

        return new HeaderTable(
            new EquatableArray<HeaderSpec>(headers.ToImmutable()),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToImmutable()));
    }

    /// <summary>
    /// Reduces the collected per-file results to the single table the emitter consumes, and
    /// reports the file-count problems that only make sense once every file has been seen.
    /// </summary>
    public static HeaderTable Merge(ImmutableArray<ParsedFile> files, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (files.Length == 0)
        {
            return HeaderTable.Empty.WithDiagnostic(new DiagnosticInfo("HHG001", string.Empty, null));
        }

        if (files.Length > 1)
        {
            // Deterministic ordering: the pipeline hands these over in whatever order the host
            // enumerated AdditionalFiles, and an unstable diagnostic message would make the
            // snapshot tests flap.
            var paths = files.Select(static f => f.Path).OrderBy(static p => p, StringComparer.Ordinal);

            return HeaderTable.Empty.WithDiagnostic(
                new DiagnosticInfo("HHG002", string.Join(", ", paths), null));
        }

        return files[0].Table;
    }

    /// <summary>
    /// Strips '-' and '.' and keeps the casing as written, so "Content-Length" becomes
    /// ContentLength and "TE" stays TE. Names where that reads badly use the "= EnumMember"
    /// override in the input file rather than a cleverer rule here.
    /// </summary>
    private static string DeriveEnumMember(string name)
    {
        var buffer = new char[name.Length];
        var length = 0;

        foreach (var c in name)
        {
            if (c != '-' && c != '.')
            {
                buffer[length++] = c;
            }
        }

        return new string(buffer, 0, length);
    }

    private static bool IsToken(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        foreach (var c in value)
        {
            var isAlpha = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            var isDigit = c >= '0' && c <= '9';

            if (!isAlpha && !isDigit && TokenPunctuation.IndexOf(c) < 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidIdentifier(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        var first = value[0];

        if (!((first >= 'a' && first <= 'z') || (first >= 'A' && first <= 'Z') || first == '_'))
        {
            return false;
        }

        foreach (var c in value)
        {
            var ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';

            if (!ok)
            {
                return false;
            }
        }

        return true;
    }
}

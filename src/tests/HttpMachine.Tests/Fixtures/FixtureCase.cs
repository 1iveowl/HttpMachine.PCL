using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpMachine.Tests.Fixtures;

/// <summary>What a fixture's path says about how to run it.</summary>
/// <remarks>
/// Everything a fixture needs beyond its bytes is encoded in its path, because the file itself must
/// stay byte-exact raw HTTP with no room for a header or a directive. The folder is the category and
/// the filename carries dot-separated modifiers:
/// <c>Responses/unframed-reads-to-eof.close-delimited.eof.http</c>.
/// </remarks>
internal sealed class FixtureCase
{
    private static readonly IReadOnlyDictionary<string, string> KnownModifiers = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["close-delimited"] = "parse with UnframedResponseMode.CloseDelimited",
        ["canonical"] = "parse with HeaderNameMode.Canonical",
        ["eof"] = "signal end of stream with a final empty Execute",
        ["invalid"] = "the parser is expected to reject the input, by stopping short or raising OnParserError",
    };

    private FixtureCase(string relativePath, string category, string name, IReadOnlyCollection<string> modifiers)
    {
        RelativePath = relativePath;
        Category = category;
        Name = name;
        Modifiers = modifiers;
    }

    public string RelativePath { get; }

    /// <summary>The folder, which is also the expected message type. See <see cref="ExpectsRequest"/>.</summary>
    public string Category { get; }

    /// <summary>The filename with modifiers and extension removed; names the snapshot.</summary>
    public string Name { get; }

    public IReadOnlyCollection<string> Modifiers { get; }

    public bool ExpectsRequest => Category == "Requests";

    public bool ExpectsResponse => Category == "Responses";

    public bool ExpectsParserError => Modifiers.Contains("invalid");

    public bool SignalsEndOfStream => Modifiers.Contains("eof");

    public UnframedResponseMode UnframedResponseMode =>
        Modifiers.Contains("close-delimited") ? UnframedResponseMode.CloseDelimited : UnframedResponseMode.CompleteAtHeaders;

    public HeaderNameMode HeaderNameMode =>
        Modifiers.Contains("canonical") ? HeaderNameMode.Canonical : HeaderNameMode.AsReceived;

    /// <summary>
    /// Parses a fixture path, throwing with an actionable message on anything malformed.
    /// </summary>
    /// <remarks>
    /// These throws are what the source generator's build-time diagnostics would have been. Failing
    /// here instead is just as loud - a mistyped modifier must never silently run the default and
    /// pass forever.
    /// </remarks>
    public static FixtureCase Parse(string relativePath)
    {
        var segments = relativePath.Split('/');

        if (segments.Length != 2)
        {
            throw new InvalidOperationException(
                $"Fixture '{relativePath}' must sit in a category folder, for example "
                + "Requests/ or Responses/, directly under Fixtures/.");
        }

        var category = segments[0];

        if (category is not ("Requests" or "Responses"))
        {
            throw new InvalidOperationException(
                $"Fixture '{relativePath}' is in unknown category folder '{category}'. "
                + "Expected Requests or Responses.");
        }

        var fileName = segments[1];

        if (!fileName.EndsWith(".http", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Fixture '{relativePath}' must end in .http.");
        }

        var parts = fileName.Substring(0, fileName.Length - ".http".Length).Split('.');
        var name = parts[0];
        var modifiers = parts.Skip(1).ToArray();

        if (name.Length == 0)
        {
            throw new InvalidOperationException($"Fixture '{relativePath}' has an empty name.");
        }

        foreach (var modifier in modifiers)
        {
            if (!KnownModifiers.ContainsKey(modifier))
            {
                throw new InvalidOperationException(
                    $"Fixture '{relativePath}' uses unknown modifier '{modifier}'. Known modifiers: "
                    + string.Join(", ", KnownModifiers.Select(m => $"{m.Key} ({m.Value})")) + ".");
            }
        }

        if (modifiers.Contains("close-delimited") && category == "Requests")
        {
            throw new InvalidOperationException(
                $"Fixture '{relativePath}' uses close-delimited, which only affects responses.");
        }

        return new FixtureCase(relativePath, category, name, modifiers);
    }
}

using System.IO;
using System.Runtime.CompilerServices;

namespace HttpMachine.Tests.Fixtures;

/// <summary>Locates the fixture folder in the source tree.</summary>
/// <remarks>
/// <para>
/// Fixtures are read straight from the source tree rather than copied to the output directory. That
/// removes a build step that could transform a byte, and it puts the approved snapshot next to the
/// fixture it describes, where a reviewer reads them together.
/// </para>
/// <para>
/// <see cref="CallerFilePathAttribute"/> is baked in at compile time and points at this file, which
/// is the only reliable anchor available: the tests run from bin/, and Verify's usual
/// source-directory resolution would land in the build output.
/// </para>
/// </remarks>
internal static class FixturePaths
{
    public static string ProjectDirectory { get; } = ResolveDirectoryOfThisFile();

    public static string FixtureRoot { get; } = Path.Combine(ProjectDirectory, "Fixtures");

    /// <summary>Where the approved snapshot for a fixture in <paramref name="category"/> lives.</summary>
    public static string SnapshotDirectory(string category) => Path.Combine(FixtureRoot, category);

    public static string FullPath(string relativePath) =>
        Path.Combine(FixtureRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string ResolveDirectoryOfThisFile([CallerFilePath] string path = "") =>
        Path.GetDirectoryName(Path.GetDirectoryName(path)!)!;
}

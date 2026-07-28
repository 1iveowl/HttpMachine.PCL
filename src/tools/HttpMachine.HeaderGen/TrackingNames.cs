namespace HttpMachine.HeaderGen;

/// <summary>
/// Names attached to pipeline stages with <c>WithTrackingName</c>.
/// </summary>
/// <remarks>
/// These exist purely so the cacheability test can pull each stage out of
/// <c>GeneratorRunResult.TrackedSteps</c> and assert that a second run reuses its result. They cost
/// nothing at run time and are the only way to observe the pipeline's caching from a test, so every
/// stage gets one.
/// </remarks>
internal static class TrackingNames
{
    public const string SelectFile = "SelectFile";
    public const string ParseFile = "ParseFile";
    public const string BuildModel = "BuildModel";

    /// <summary>Every tracking name. All of these must report Cached or Unchanged on a re-run.</summary>
    public static readonly string[] All = { SelectFile, ParseFile, BuildModel };

    /// <summary>
    /// The stages carrying values this generator defines, as opposed to values Roslyn defines.
    /// </summary>
    /// <remarks>
    /// <see cref="SelectFile"/> is excluded deliberately. It is an
    /// <c>AdditionalTextsProvider.Where</c>, so its element type is <c>AdditionalText</c> - a
    /// Roslyn reference type - and no amount of care on our side changes that. It is safe there
    /// precisely because the driver, not this generator, owns those instances: the host hands back
    /// the same <c>AdditionalText</c> when the file has not changed, which is what lets the stage
    /// report Cached. The rule being enforced is that no such type may escape into a model we
    /// build, which starts at <see cref="ParseFile"/>.
    /// </remarks>
    public static readonly string[] ValueModelStages = { ParseFile, BuildModel };
}

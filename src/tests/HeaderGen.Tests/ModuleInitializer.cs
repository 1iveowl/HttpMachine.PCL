using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace HeaderGen.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Teaches Verify how to serialise a GeneratorDriverRunResult. Without it the snapshots
        // come out as an object dump rather than the generated source and its diagnostics.
        VerifySourceGenerators.Initialize();

        // Never try to pop a diff tool: these tests run headless in CI and in containers.
        DiffRunner.Disabled = true;
    }
}

using System.Runtime.CompilerServices;
using DiffEngine;

namespace HttpMachine.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Never try to pop a diff tool: these tests run headless in CI and in containers.
        DiffRunner.Disabled = true;
    }
}

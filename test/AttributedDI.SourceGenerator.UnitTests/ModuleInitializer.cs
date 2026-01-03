using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AttributedDI.SourceGenerator.UnitTests;

/// <summary>
/// Initializes settings for the test assembly.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes Verify settings including source generator support and default snapshot directory.
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();

        // Configure all snapshots to be stored in the "Snapshots" directory by default
        DerivePathInfo((_, projectDirectory, type, method) =>
            new PathInfo(
                directory: Path.Combine(projectDirectory, "Snapshots", GetTfmSubdirectory()),
                typeName: type.Name,
                methodName: method.Name));
    }

    private static string GetTfmSubdirectory()
    {
        // Prefer compile-time TFMs for deterministic folder naming
#if NET10_0_OR_GREATER
        return "net10.0";
#elif NET9_0_OR_GREATER
        return "net9.0";
#elif NET8_0_OR_GREATER
        return "net8.0";
#else
        var tfm = RuntimeInformation.FrameworkDescription.Replace(' ', '_');
        return string.IsNullOrWhiteSpace(tfm) ? "unknown" : tfm;
#endif
    }
}
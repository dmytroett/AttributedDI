using System.Runtime.CompilerServices;

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
                directory: Path.Combine(projectDirectory, "Snapshots"),
                typeName: type.Name,
                methodName: method.Name));
    }
}
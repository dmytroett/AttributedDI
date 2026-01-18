using CliWrap;
using CliWrap.Buffered;

namespace AttributedDI.E2ETests;

public class LocalPackageE2ETests
{
    [Fact]
    public async Task PackagedLibraryCanBeRestoredAndUsed()
    {
        string repoRoot = FindRepoRoot();
        string testRoot = Path.Combine(Path.GetTempPath(), "AttributedDI.E2E", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testRoot);

        try
        {
            string localFeed = Path.Combine(testRoot, "local-feed");
            string packagesFolder = Path.Combine(testRoot, "packages");
            string nugetConfig = Path.Combine(testRoot, "nuget.config");
            Directory.CreateDirectory(localFeed);
            Directory.CreateDirectory(packagesFolder);

            string packageVersion = $"99.0.0-e2e.{Guid.NewGuid():N}";

            await RunDotnetAsync(
                repoRoot,
                "pack",
                Path.Combine(repoRoot, "src", "AttributedDI", "AttributedDI.csproj"),
                "-c",
                "Release",
                $"-p:PackageVersion={packageVersion}",
                "-o",
                localFeed);

            WriteNugetConfig(nugetConfig, localFeed, packagesFolder);

            string consumerTestsProject = Path.Combine(
                repoRoot,
                "test",
                "e2e",
                "AllModulesRegistration.ConsumerTests",
                "AllModulesRegistration.ConsumerTests.csproj");

            await RunDotnetAsync(
                repoRoot,
                "test",
                consumerTestsProject,
                "--configfile",
                nugetConfig,
                "--framework",
                GetCurrentTargetFramework(),
                $"-p:AttributedDIPackageVersion={packageVersion}");
        }
        finally
        {
            TryDeleteDirectory(testRoot);
        }
    }

    private static string GetCurrentTargetFramework()
    {
        string? targetFramework = AppContext.TargetFrameworkName;
        if (string.IsNullOrWhiteSpace(targetFramework))
        {
            throw new InvalidOperationException("Target framework information is unavailable.");
        }

        const string versionToken = "Version=v";
        int versionIndex = targetFramework.IndexOf(versionToken, StringComparison.OrdinalIgnoreCase);
        if (versionIndex < 0)
        {
            throw new InvalidOperationException($"Unexpected target framework name: {targetFramework}");
        }

        string version = targetFramework[(versionIndex + versionToken.Length)..];
        return $"net{version}";
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "AttributedDI.slnx")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private static void WriteNugetConfig(string configPath, string localFeed, string packagesFolder)
    {
        string config = $"""
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="{localFeed}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="AttributedDI" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
  <config>
    <globalPackagesFolder>{packagesFolder}</globalPackagesFolder>
  </config>
</configuration>
""";

        File.WriteAllText(configPath, config);
    }

    private static async Task RunDotnetAsync(string workingDirectory, params string[] arguments)
    {
        BufferedCommandResult result = await Cli.Wrap("dotnet")
            .WithWorkingDirectory(workingDirectory)
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        if (result.ExitCode != 0)
        {
            string joinedArgs = string.Join(' ', arguments);
            throw new InvalidOperationException(
                $"dotnet {joinedArgs} failed with exit code {result.ExitCode}{Environment.NewLine}" +
                $"STDOUT:{Environment.NewLine}{result.StandardOutput}{Environment.NewLine}" +
                $"STDERR:{Environment.NewLine}{result.StandardError}");
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Cleanup failures should not mask test results.
        }
        catch (UnauthorizedAccessException)
        {
            // Cleanup failures should not mask test results.
        }
    }
}
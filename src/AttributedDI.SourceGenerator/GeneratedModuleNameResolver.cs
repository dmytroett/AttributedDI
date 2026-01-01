using System;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves generated module names and registration method names from assembly names and custom naming information.
/// </summary>
internal static class GeneratedModuleNameResolver
{
    /// <summary>
    /// Resolves the generated module class name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="customNameInfo">Custom module name information from attributes, or null to use defaults.</param>
    /// <returns>The module class name (e.g., "CompanyNameTeamNameProjectNameAPIModule" or "MyFeatureModule").</returns>
    public static string ResolveModuleName(string assemblyName, CustomModuleNameInfo? customNameInfo)
    {
        string baseName = customNameInfo?.ModuleName ?? $"{assemblyName}Module";
        string sanitized = SanitizeIdentifier(baseName);
        return sanitized;
    }

    /// <summary>
    /// Resolves the registration method name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="customNameInfo">Custom module name information from attributes, or null to use defaults.</param>
    /// <returns>The method name (e.g., "AddCompanyNameTeamNameProjectNameAPI" or "AddMyFeature").</returns>
    public static string ResolveMethodName(string assemblyName, CustomModuleNameInfo? customNameInfo)
    {
        string baseName;

        if (customNameInfo?.MethodName != null)
        {
            // Explicit method name provided
            baseName = customNameInfo.MethodName;
        }
        else
        {
            // No method name provided - use default "Add" + assembly name
            baseName = $"Add{assemblyName}";
        }

        string sanitized = SanitizeIdentifier(baseName);
        return sanitized;
    }

    private static string SanitizeIdentifier(string name)
    {
        // Remove invalid characters to create a valid C# identifier
        string sanitized = new([.. name.Where(c => char.IsLetterOrDigit(c) || c == '_')]);

        // Ensure identifier doesn't start with a digit
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }
}
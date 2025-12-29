using Microsoft.CodeAnalysis;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves registration extension method names from assembly names and aliases.
/// </summary>
internal static class RegistrationMethodNameResolver
{
    /// <summary>
    /// Extracts assembly alias information from the attribute context.
    /// </summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <returns>Assembly alias info, or null if extraction fails.</returns>
    public static AssemblyAliasInfo? GetAssemblyAliasInfo(GeneratorAttributeSyntaxContext context)
    {
        var attribute = context.Attributes[0];

        // Get method name (first constructor argument)
        if (attribute.ConstructorArguments.Length < 1)
        {
            return null;
        }

        var aliasArg = attribute.ConstructorArguments[0];
        if (aliasArg.Value is not string methodName || string.IsNullOrWhiteSpace(methodName))
        {
            return null;
        }

        // For assembly-level attributes, the TargetSymbol is the assembly itself
        var assemblySymbol = context.TargetSymbol as IAssemblySymbol ?? context.TargetSymbol.ContainingAssembly;

        if (assemblySymbol is null)
        {
            return null;
        }

        return new AssemblyAliasInfo(methodName, assemblySymbol.Name);
    }

    /// <summary>
    /// Resolves the registration method name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="alias">Optional alias information from RegistrationMethodNameAttribute.</param>
    /// <returns>The method name (e.g., "AddCompanyNameTeamNameProjectNameAPI" or "AddMyFeature").</returns>
    public static string Resolve(string assemblyName, AssemblyAliasInfo? alias)
    {
        string baseName = alias?.MethodName ?? assemblyName;
        string sanitized = SanitizeIdentifier(baseName);
        return $"Add{sanitized}";
    }

    private static string SanitizeIdentifier(string name)
    {
        // Remove invalid characters from name to create a valid C# identifier
        string sanitized = new([.. name.Where(c => char.IsLetterOrDigit(c) || c == '_')]);

        // Ensure it doesn't start with a digit
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }
}
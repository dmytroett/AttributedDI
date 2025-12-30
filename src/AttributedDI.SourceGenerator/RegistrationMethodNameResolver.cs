using Microsoft.CodeAnalysis;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves registration extension method names from assembly names and aliases.
/// </summary>
internal static class RegistrationMethodNameResolver
{
    private const string UnknownAssemblyName = "UnknownAssembly";

    /// <summary>
    /// Scans the assembly for registration method name information.
    /// Always emits one AssemblyInfo per compilation; if no attribute is present, MethodName is null.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental value provider of assembly information.</returns>
    public static IncrementalValueProvider<AssemblyInfo> ScanAssembly(
        IncrementalGeneratorInitializationContext context)
    {
        var aliases = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                KnownAttributes.RegistrationMethodNameAttribute,
                static (_, _) => true,
                static (ctx, _) => GetAssemblyAliasInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Create default AssemblyInfo from compilation, preferring alias if present
        return context.CompilationProvider.Combine(aliases.Collect())
            .Select(static (pair, _) =>
            {
                var compilation = pair.Left;
                var aliasesList = pair.Right;

                // Prefer attribute-derived alias; otherwise use assembly name from compilation
                return aliasesList.FirstOrDefault()
                    ?? new AssemblyInfo(null, compilation.AssemblyName ?? UnknownAssemblyName);
            });
    }

    /// <summary>
    /// Extracts assembly alias information from the attribute context.
    /// </summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <returns>Assembly information with alias, or null if extraction fails.</returns>
    private static AssemblyInfo? GetAssemblyAliasInfo(GeneratorAttributeSyntaxContext context)
    {
        var attribute = context.Attributes[0];
        if (attribute.ConstructorArguments.Length < 1)
            return null;

        var aliasArg = attribute.ConstructorArguments[0];
        if (aliasArg.Value is not string methodName || string.IsNullOrWhiteSpace(methodName))
            return null;

        var assemblySymbol = context.TargetSymbol as IAssemblySymbol ?? context.TargetSymbol.ContainingAssembly;
        return assemblySymbol is null ? null : new AssemblyInfo(methodName, assemblySymbol.Name);
    }

    /// <summary>
    /// Resolves the registration method name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="assemblyInfo">Assembly information including optional method name override.</param>
    /// <returns>The method name (e.g., "AddCompanyNameTeamNameProjectNameAPI" or "AddMyFeature").</returns>
    public static string Resolve(string assemblyName, AssemblyInfo assemblyInfo)
    {
        string baseName = assemblyInfo.MethodName ?? assemblyName;
        string sanitized = SanitizeIdentifier(baseName);
        return $"Add{sanitized}";
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
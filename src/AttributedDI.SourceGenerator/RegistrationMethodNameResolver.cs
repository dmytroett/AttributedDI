using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves registration extension method names from assembly names and aliases.
/// </summary>
internal static class RegistrationMethodNameResolver
{
    /// <summary>
    /// Scans the assembly for registration method name information.
    /// Always emits one AssemblyInfo per compilation; if no attribute is present, MethodName is null.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental value provider of assembly information.</returns>
    public static IncrementalValueProvider<AssemblyInfo> ScanAssembly(
        IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider.Select(static (compilation, _) =>
        {
            string? methodName = TryGetMethodNameFromAssemblyAttributes(compilation);
            return new AssemblyInfo(methodName, compilation.Assembly.Name);
        });
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

    private static string? TryGetMethodNameFromAssemblyAttributes(Compilation compilation)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(KnownAttributes.RegistrationMethodNameAttribute);
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            if (!IsRegistrationMethodNameAttribute(attributeClass, attributeSymbol))
            {
                continue;
            }

            if (attribute.ConstructorArguments is [{ Value: string methodName }] && !string.IsNullOrWhiteSpace(methodName))
            {
                return methodName;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the given attribute class is a RegistrationMethodNameAttribute.
    /// Uses multiple matching strategies: symbol equality, full name, and short name.
    /// </summary>
    /// <param name="attributeClass">The attribute class to check.</param>
    /// <param name="attributeSymbol">The resolved symbol for RegistrationMethodNameAttribute, if available.</param>
    /// <returns>True if the attribute is a RegistrationMethodNameAttribute; otherwise, false.</returns>
    private static bool IsRegistrationMethodNameAttribute(INamedTypeSymbol attributeClass, INamedTypeSymbol? attributeSymbol)
    {
        // Strategy 1: Symbol equality (most reliable when symbol resolution succeeds)
        if (attributeSymbol is not null && SymbolEqualityComparer.Default.Equals(attributeClass, attributeSymbol))
        {
            return true;
        }

        // Strategy 2: Full name match (works across assembly boundaries)
        if (string.Equals(attributeClass.ToDisplayString(), KnownAttributes.RegistrationMethodNameAttribute, StringComparison.Ordinal))
        {
            return true;
        }

        // Strategy 3: Short name match (handles aliased attributes)
        if (string.Equals(attributeClass.Name, "RegistrationMethodNameAttribute", StringComparison.Ordinal) ||
            string.Equals(attributeClass.Name, "RegistrationMethodName", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
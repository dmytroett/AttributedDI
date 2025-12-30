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

            bool matchesSymbol = attributeSymbol is not null && SymbolEqualityComparer.Default.Equals(attributeClass, attributeSymbol);
            bool matchesFullName = string.Equals(attributeClass.ToDisplayString(), KnownAttributes.RegistrationMethodNameAttribute, StringComparison.Ordinal);
            bool matchesShortName = string.Equals(attributeClass.Name, "RegistrationMethodNameAttribute", StringComparison.Ordinal) ||
                                   string.Equals(attributeClass.Name, "RegistrationMethodName", StringComparison.Ordinal);
            if (!matchesSymbol && !matchesFullName && !matchesShortName)
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
}
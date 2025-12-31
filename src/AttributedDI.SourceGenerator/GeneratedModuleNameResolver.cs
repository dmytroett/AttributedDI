using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves generated module names and registration method names from assembly names and attributes.
/// </summary>
internal static class GeneratedModuleNameResolver
{
    /// <summary>
    /// Scans the assembly for generated module name information.
    /// Always emits one AssemblyInfo per compilation; if no attribute is present, ModuleName and MethodName are null.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental value provider of assembly information.</returns>
    public static IncrementalValueProvider<AssemblyInfo> ScanAssembly(
        IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider.Select(static (compilation, _) =>
        {
            var (moduleName, methodName) = TryGetNamesFromAssemblyAttributes(compilation);
            return new AssemblyInfo(moduleName, methodName, compilation.Assembly.Name);
        });
    }

    /// <summary>
    /// Resolves the generated module class name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="assemblyInfo">Assembly information including optional module name override.</param>
    /// <returns>The module class name (e.g., "CompanyNameTeamNameProjectNameAPIModule" or "MyFeatureModule").</returns>
    public static string ResolveModuleName(string assemblyName, AssemblyInfo assemblyInfo)
    {
        string baseName = assemblyInfo.ModuleName ?? $"{assemblyName}Module";
        string sanitized = SanitizeIdentifier(baseName);
        return sanitized;
    }

    /// <summary>
    /// Resolves the registration method name for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="assemblyInfo">Assembly information including optional method name override.</param>
    /// <returns>The method name (e.g., "AddCompanyNameTeamNameProjectNameAPI" or "AddMyFeature").</returns>
    public static string ResolveMethodName(string assemblyName, AssemblyInfo assemblyInfo)
    {
        string baseName;

        if (assemblyInfo.MethodName != null)
        {
            // Explicit method name provided
            baseName = assemblyInfo.MethodName;
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

    private static (string? ModuleName, string? MethodName) TryGetNamesFromAssemblyAttributes(Compilation compilation)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(KnownAttributes.GeneratedModuleNameAttribute);
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            if (!IsGeneratedModuleNameAttribute(attributeClass, attributeSymbol))
            {
                continue;
            }

            string? moduleName = null;
            string? methodName = null;

            // Extract constructor arguments (both are optional)
            if (attribute.ConstructorArguments.Length >= 1)
            {
                var moduleNameArg = attribute.ConstructorArguments[0];
                if (!moduleNameArg.IsNull && moduleNameArg.Value is string mn && !string.IsNullOrWhiteSpace(mn))
                {
                    moduleName = mn;
                }
            }

            if (attribute.ConstructorArguments.Length >= 2)
            {
                var methodNameArg = attribute.ConstructorArguments[1];
                if (!methodNameArg.IsNull && methodNameArg.Value is string tn && !string.IsNullOrWhiteSpace(tn))
                {
                    methodName = tn;
                }
            }

            // Also check named arguments
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "ModuleName" && !namedArg.Value.IsNull && namedArg.Value.Value is string mn2 && !string.IsNullOrWhiteSpace(mn2))
                {
                    moduleName = mn2;
                }
                else if (namedArg.Key == "MethodName" && !namedArg.Value.IsNull && namedArg.Value.Value is string tn2 && !string.IsNullOrWhiteSpace(tn2))
                {
                    methodName = tn2;
                }
            }

            return (moduleName, methodName);
        }

        return (null, null);
    }

    /// <summary>
    /// Determines whether the given attribute class is a GeneratedModuleNameAttribute.
    /// Uses multiple matching strategies: symbol equality, full name, and short name.
    /// </summary>
    /// <param name="attributeClass">The attribute class to check.</param>
    /// <param name="attributeSymbol">The resolved symbol for GeneratedModuleNameAttribute, if available.</param>
    /// <returns>True if the attribute is a GeneratedModuleNameAttribute; otherwise, false.</returns>
    private static bool IsGeneratedModuleNameAttribute(INamedTypeSymbol attributeClass, INamedTypeSymbol? attributeSymbol)
    {
        // Strategy 1: Symbol equality (most reliable when symbol resolution succeeds)
        if (attributeSymbol is not null && SymbolEqualityComparer.Default.Equals(attributeClass, attributeSymbol))
        {
            return true;
        }

        // Strategy 2: Full name match (works across assembly boundaries)
        if (string.Equals(attributeClass.ToDisplayString(), KnownAttributes.GeneratedModuleNameAttribute, StringComparison.Ordinal))
        {
            return true;
        }

        // Strategy 3: Short name match (handles aliased attributes)
        if (string.Equals(attributeClass.Name, "GeneratedModuleNameAttribute", StringComparison.Ordinal) ||
            string.Equals(attributeClass.Name, "GeneratedModuleName", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
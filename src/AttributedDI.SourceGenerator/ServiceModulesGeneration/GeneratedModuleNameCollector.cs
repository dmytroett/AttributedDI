using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

/// <summary>
/// Collects custom module name information from assembly-level attributes.
/// </summary>
internal static class GeneratedModuleNameCollector
{
    /// <summary>
    /// Scans the assembly for custom module name information from GeneratedModuleNameAttribute.
    /// Returns null for all properties if no attribute is present.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental value provider of custom module name information.</returns>
    public static IncrementalValueProvider<ResolvedModuleNames> Collect(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Assembly.Name);

        var customModuleNameAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                KnownAttributes.GeneratedModuleNameAttribute,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: CollectUserDefinedNames)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        return assemblyName
            .Combine(customModuleNameAttribute.Collect())
            .Select(static (data, _) =>
            {
                var customName = data.Right.FirstOrDefault();
                var assembly = data.Left;

                string moduleName = GeneratedModuleNameResolver.ResolveModuleName(assembly, customName);
                string methodName = GeneratedModuleNameResolver.ResolveMethodName(assembly, customName);
                string moduleNamespace = GeneratedModuleNameResolver.ResolveNamespace(assembly, customName);

                return new ResolvedModuleNames(moduleName, methodName, moduleNamespace);
            });
    }

    private static CustomModuleNameInfo? CollectUserDefinedNames(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var attributeData = context.Attributes
            .FirstOrDefault(attr => attr.AttributeClass is not null && IsGeneratedModuleNameAttribute(attr.AttributeClass));

        if (attributeData is null)
        {
            return null;
        }

        token.ThrowIfCancellationRequested();

        string? moduleName = null;
        string? methodName = null;
        string? moduleNamespace = null;

        // Extract constructor arguments (both are optional)
        if (attributeData.ConstructorArguments.Length >= 1)
        {
            var moduleNameArg = attributeData.ConstructorArguments[0];
            if (!moduleNameArg.IsNull && moduleNameArg.Value is string mn && !string.IsNullOrWhiteSpace(mn))
            {
                moduleName = mn;
            }
        }

        if (attributeData.ConstructorArguments.Length >= 2)
        {
            var methodNameArg = attributeData.ConstructorArguments[1];
            if (!methodNameArg.IsNull && methodNameArg.Value is string tn && !string.IsNullOrWhiteSpace(tn))
            {
                methodName = tn;
            }
        }

        if (attributeData.ConstructorArguments.Length >= 3)
        {
            var namespaceArg = attributeData.ConstructorArguments[2];
            if (!namespaceArg.IsNull && namespaceArg.Value is string ns && !string.IsNullOrWhiteSpace(ns))
            {
                moduleNamespace = ns;
            }
        }

        // Also check named arguments
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "ModuleName" && !namedArg.Value.IsNull && namedArg.Value.Value is string mn2 && !string.IsNullOrWhiteSpace(mn2))
            {
                moduleName = mn2;
            }
            else if (namedArg.Key == "MethodName" && !namedArg.Value.IsNull && namedArg.Value.Value is string tn2 && !string.IsNullOrWhiteSpace(tn2))
            {
                methodName = tn2;
            }
            else if ((namedArg.Key == "Namespace" || namedArg.Key == "ModuleNamespace") &&
                !namedArg.Value.IsNull && namedArg.Value.Value is string ns2 && !string.IsNullOrWhiteSpace(ns2))
            {
                moduleNamespace = ns2;
            }
        }

        return new CustomModuleNameInfo(moduleName, methodName, moduleNamespace);
    }

    /// <summary>
    /// Determines whether the given attribute class is a GeneratedModuleNameAttribute.
    /// Uses multiple matching strategies: symbol equality, full name, and short name.
    /// </summary>
    /// <param name="attributeClass">The attribute class to check.</param>
    /// <returns>True if the attribute is a GeneratedModuleNameAttribute; otherwise, false.</returns>
    private static bool IsGeneratedModuleNameAttribute(INamedTypeSymbol attributeClass)
    {
        if (string.Equals(attributeClass.ToDisplayString(), KnownAttributes.GeneratedModuleNameAttribute, StringComparison.Ordinal) &&
            string.Equals(attributeClass.Name, "GeneratedModuleNameAttribute", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
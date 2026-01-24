using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;

/// <summary>
/// Collects custom service-collection extension naming information from assembly-level attributes.
/// </summary>
internal static class ServiceCollectionExtensionNameCollector
{
    /// <summary>
    /// Scans the assembly for custom extension naming information from ServiceCollectionExtensionAttribute.
    /// Returns null for all properties if no attribute is present.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental value provider of custom extension naming information.</returns>
    public static IncrementalValueProvider<ResolvedExtensionNames> Collect(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Assembly.Name);

        var customExtensionNameAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                KnownAttributes.ServiceCollectionExtensionAttribute,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: CollectUserDefinedNames)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        return assemblyName
            .Combine(customExtensionNameAttribute.Collect())
            .Select(static (data, _) =>
            {
                var customName = data.Right.FirstOrDefault();
                var assembly = data.Left;

                return ServiceCollectionExtensionNameResolver.Resolve(assembly, customName);
            });
    }

    private static CustomExtensionNameInfo? CollectUserDefinedNames(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var attributeData = context.Attributes
            .FirstOrDefault(attr => attr.AttributeClass is not null && IsServiceCollectionExtensionAttribute(attr.AttributeClass));

        if (attributeData is null)
        {
            return null;
        }

        token.ThrowIfCancellationRequested();

        string? extensionClassName = null;
        string? methodName = null;
        string? extensionNamespace = null;

        // Extract constructor arguments (both are optional)
        if (attributeData.ConstructorArguments.Length >= 1)
        {
            var extensionClassNameArg = attributeData.ConstructorArguments[0];
            if (!extensionClassNameArg.IsNull &&
                extensionClassNameArg.Value is string cn &&
                !string.IsNullOrWhiteSpace(cn))
            {
                extensionClassName = cn;
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
                extensionNamespace = ns;
            }
        }

        // Also check named arguments
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "ExtensionClassName" &&
                !namedArg.Value.IsNull &&
                namedArg.Value.Value is string cn2 &&
                !string.IsNullOrWhiteSpace(cn2))
            {
                extensionClassName = cn2;
            }
            else if (namedArg.Key == "MethodName" && !namedArg.Value.IsNull && namedArg.Value.Value is string tn2 && !string.IsNullOrWhiteSpace(tn2))
            {
                methodName = tn2;
            }
            else if (namedArg.Key == "ExtensionNamespace" &&
                !namedArg.Value.IsNull && namedArg.Value.Value is string ns2 && !string.IsNullOrWhiteSpace(ns2))
            {
                extensionNamespace = ns2;
            }
        }

        return new CustomExtensionNameInfo(extensionClassName, methodName, extensionNamespace);
    }

    /// <summary>
    /// Determines whether the given attribute class is a ServiceCollectionExtensionAttribute.
    /// </summary>
    /// <param name="attributeClass">The attribute class to check.</param>
    /// <returns>True if the attribute is a ServiceCollectionExtensionAttribute; otherwise, false.</returns>
    private static bool IsServiceCollectionExtensionAttribute(INamedTypeSymbol attributeClass)
    {
        return string.Equals(attributeClass.ToDisplayString(), KnownAttributes.ServiceCollectionExtensionAttribute, StringComparison.Ordinal) &&
               string.Equals(attributeClass.Name, "ServiceCollectionExtensionAttribute", StringComparison.Ordinal);
    }
}

internal sealed record CustomExtensionNameInfo(string? ExtensionClassName, string? MethodName, string? ExtensionNamespace);

internal sealed record ResolvedExtensionNames(string ExtensionClassName, string MethodName, string Namespace);

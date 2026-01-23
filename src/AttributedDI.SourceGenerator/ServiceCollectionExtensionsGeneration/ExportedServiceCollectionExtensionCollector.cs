using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionsGeneration;

internal static class ExportedServiceCollectionExtensionCollector
{
    public static ImmutableArray<ExportedServiceCollectionExtensionInfo> CollectFromReferences(
        Compilation compilation,
        CancellationToken token)
    {
        var results = new List<ExportedServiceCollectionExtensionInfo>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reference in compilation.References)
        {
            token.ThrowIfCancellationRequested();

            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(assemblySymbol, compilation.Assembly))
            {
                continue;
            }

            CollectFromAssembly(assemblySymbol, seen, results);
        }

        if (results.Count == 0)
        {
            return ImmutableArray<ExportedServiceCollectionExtensionInfo>.Empty;
        }

        return results
            .OrderBy(static info => info.FullyQualifiedTypeName, StringComparer.Ordinal)
            .ThenBy(static info => info.MethodName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static void CollectFromAssembly(
        IAssemblySymbol assemblySymbol,
        HashSet<string> seen,
        List<ExportedServiceCollectionExtensionInfo> results)
    {
        foreach (var attribute in assemblySymbol.GetAttributes())
        {
            if (attribute.AttributeClass is null ||
                !string.Equals(attribute.AttributeClass.ToDisplayString(), KnownAttributes.ExportsServiceCollectionExtensionAttribute, StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryCreateExport(attribute, out var info))
            {
                continue;
            }

            string key = $"{info!.FullyQualifiedTypeName}|{info.MethodName}";
            if (seen.Add(key))
            {
                results.Add(info);
            }
        }
    }

    private static bool TryCreateExport(AttributeData attribute, out ExportedServiceCollectionExtensionInfo? info)
    {
        info = default;

        if (attribute.ConstructorArguments.Length < 2)
        {
            return false;
        }

        var typeArg = attribute.ConstructorArguments[0];
        var methodArg = attribute.ConstructorArguments[1];

        if (typeArg.Kind != TypedConstantKind.Type ||
            typeArg.Value is not INamedTypeSymbol typeSymbol ||
            methodArg.Value is not string methodName ||
            string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        string fullyQualifiedTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        info = new ExportedServiceCollectionExtensionInfo(fullyQualifiedTypeName, methodName);
        return true;
    }
}

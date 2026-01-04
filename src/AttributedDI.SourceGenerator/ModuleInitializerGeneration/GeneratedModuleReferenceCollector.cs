using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator.ModuleInitializerGeneration;

internal static class GeneratedModuleReferenceCollector
{
    public static ImmutableArray<GeneratedModuleRegistrationInfo> CollectGeneratedModulesFromReferences(Compilation compilation, CancellationToken token)
    {
        var serviceModuleSymbol = compilation.GetTypeByMetadataName("AttributedDI.IServiceModule");
        if (serviceModuleSymbol is null)
        {
            return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
        }

        var moduleNames = new HashSet<string>(StringComparer.Ordinal);

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

            foreach (var type in EnumerateTypes(assemblySymbol.GlobalNamespace, token))
            {
                token.ThrowIfCancellationRequested();

                if (!IsGeneratedModule(type, serviceModuleSymbol))
                {
                    continue;
                }

                var fullyQualifiedName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                _ = moduleNames.Add(fullyQualifiedName);
            }
        }

        if (moduleNames.Count == 0)
        {
            return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
        }

        return moduleNames
            .OrderBy(static name => name, StringComparer.Ordinal)
            .Select(static name => new GeneratedModuleRegistrationInfo(name))
            .ToImmutableArray();
    }

    private static bool IsGeneratedModule(INamedTypeSymbol type, INamedTypeSymbol serviceModuleSymbol)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract)
        {
            return false;
        }

        if (!type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, serviceModuleSymbol)))
        {
            return false;
        }

        foreach (var attribute in type.GetAttributes())
        {
            if (attribute.AttributeClass is null)
            {
                continue;
            }

            if (string.Equals(attribute.AttributeClass.ToDisplayString(), KnownAttributes.GeneratedModuleAttribute, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol @namespace, CancellationToken token)
    {
        foreach (var member in @namespace.GetMembers())
        {
            token.ThrowIfCancellationRequested();

            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var nestedType in EnumerateTypes(nestedNamespace, token))
                {
                    yield return nestedType;
                }
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;

                foreach (var nestedType in EnumerateNestedTypes(type, token))
                {
                    yield return nestedType;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol type, CancellationToken token)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            token.ThrowIfCancellationRequested();

            yield return nested;

            foreach (var deeper in EnumerateNestedTypes(nested, token))
            {
                yield return deeper;
            }
        }
    }
}
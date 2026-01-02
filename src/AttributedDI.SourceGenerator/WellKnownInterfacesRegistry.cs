using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Provides lookups for well-known framework interfaces and their members to avoid redundant generation.
/// </summary>
internal static class WellKnownInterfacesRegistry
{
    private static readonly SymbolDisplayFormat MemberDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParamsRefOut,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly ImmutableHashSet<string> WellKnownInterfaceMetadataNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "global::System.IDisposable",
        "global::System.IAsyncDisposable",
        "global::System.IComparable",
        "global::System.IComparable`1",
        "global::System.IEquatable`1",
        "global::System.Collections.IEnumerable",
        "global::System.Collections.Generic.IEnumerable`1",
        "global::System.Collections.Generic.IAsyncEnumerable`1");

    /// <summary>
    /// Gets the display format used for member signature comparisons.
    /// </summary>
    internal static SymbolDisplayFormat InterfaceMemberDisplayFormat => MemberDisplayFormat;

    internal static bool IsWellKnownInterface(ITypeSymbol interfaceSymbol)
    {
        if (interfaceSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var metadataName = namedTypeSymbol.IsGenericType
            ? namedTypeSymbol.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return WellKnownInterfaceMetadataNames.Contains(metadataName);
    }

    internal static ImmutableHashSet<string> GetImplementedMemberSignatures(INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
        {
            if (!IsWellKnownInterface(interfaceSymbol))
            {
                continue;
            }

            foreach (var interfaceMember in interfaceSymbol.GetMembers())
            {
                var implementation = typeSymbol.FindImplementationForInterfaceMember(interfaceMember);
                if (implementation is null)
                {
                    continue;
                }

                var signature = implementation.ToDisplayString(MemberDisplayFormat);
                builder.Add(signature);
            }
        }

        return builder.ToImmutable();
    }
}
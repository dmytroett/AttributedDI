using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

internal static class GeneratedInterfacesCollector
{
    public static IncrementalValuesProvider<GeneratedInterfaceInfo> Collect(IncrementalGeneratorInitializationContext context)
    {
        var generateInterfaceTypes = CreateCollector(context, KnownAttributes.GenerateInterfaceAttribute);
        var registerAsGeneratedInterfaceTypes = CreateCollector(context, KnownAttributes.RegisterAsGeneratedInterfaceAttribute);

        return generateInterfaceTypes
            .Collect()
            .Combine(registerAsGeneratedInterfaceTypes.Collect())
            .Select(static (pair, _) => pair.Left.AddRange(pair.Right))
            .SelectMany(static (items, _) => items);
    }

    private static IncrementalValuesProvider<GeneratedInterfaceInfo> CreateCollector(
        IncrementalGeneratorInitializationContext context,
        string attributeMetadataName)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: attributeMetadataName,
                predicate: static (_, _) => true,
                transform: static (ctx, ct) => Transform(ctx, ct))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);
    }

    private static GeneratedInterfaceInfo? Transform(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        if (!GeneratedInterfaceNamingResolver.TryResolve(typeSymbol, context.Attributes[0], out var naming) || naming is null)
        {
            return null;
        }

        var resolvedNaming = naming;
        var members = CollectMembers(typeSymbol, ct);
        var accessibility = ResolveAccessibility(typeSymbol.DeclaredAccessibility);

        return new GeneratedInterfaceInfo(resolvedNaming.InterfaceName, resolvedNaming.InterfaceNamespace, accessibility, members);
    }

    private static ImmutableArray<string> CollectMembers(INamedTypeSymbol typeSymbol, CancellationToken ct)
    {
        var membersToSkip = WellKnownInterfacesRegistry.GetImplementedMemberSignatures(typeSymbol);
        var membersBuilder = ImmutableArray.CreateBuilder<string>();
        var seenMembers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var member in typeSymbol.GetMembers())
        {
            ct.ThrowIfCancellationRequested();

            if (member.IsImplicitlyDeclared || member.IsStatic)
            {
                continue;
            }

            switch (member)
            {
                case IMethodSymbol method when ShouldIgnoreMethod(method):
                    continue;
                case IMethodSymbol method when method.DeclaredAccessibility == Accessibility.Public:
                    AddMemberIfNeeded(method, membersToSkip, membersBuilder, seenMembers);
                    break;
                case IPropertySymbol property when property.DeclaredAccessibility == Accessibility.Public:
                    AddMemberIfNeeded(property, membersToSkip, membersBuilder, seenMembers);
                    break;
            }
        }

        return membersBuilder.ToImmutable();
    }

    private static void AddMemberIfNeeded(
        ISymbol member,
        ImmutableHashSet<string> membersToSkip,
        ImmutableArray<string>.Builder membersBuilder,
        HashSet<string> seenMembers)
    {
        var signature = member.ToDisplayString(WellKnownInterfacesRegistry.InterfaceMemberDisplayFormat);
        if (membersToSkip.Contains(signature))
        {
            return;
        }

        if (seenMembers.Add(signature))
        {
            membersBuilder.Add(signature);
        }
    }

    private static bool ShouldIgnoreMethod(IMethodSymbol method)
    {
        if (method.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        return method.MethodKind is MethodKind.Constructor
            or MethodKind.StaticConstructor
            or MethodKind.Destructor
            or MethodKind.PropertyGet
            or MethodKind.PropertySet
            or MethodKind.EventAdd
            or MethodKind.EventRemove
            or MethodKind.EventRaise
            or MethodKind.BuiltinOperator
            or MethodKind.UserDefinedOperator
            or MethodKind.Conversion
            or MethodKind.LambdaMethod
            or MethodKind.LocalFunction
            or MethodKind.AnonymousFunction;
    }

    private static string ResolveAccessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            _ => "internal"
        };
    }
}
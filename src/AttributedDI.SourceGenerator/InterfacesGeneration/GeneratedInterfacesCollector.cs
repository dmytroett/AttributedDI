using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

internal static class GeneratedInterfacesCollector
{
    public static IncrementalValuesProvider<GeneratedInterfaceInfo> Collect(IncrementalGeneratorInitializationContext context)
    {
        var generateInterfaceTypes = CreateCollector(context, KnownAttributes.GenerateInterfaceAttribute);
        var registerAsGeneratedInterfaceTypes = CreateCollector(context, KnownAttributes.RegisterAsGeneratedInterfaceAttribute);

        return AggregateIncrementalProviders(generateInterfaceTypes, registerAsGeneratedInterfaceTypes);
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

        if (typeSymbol.ContainingType is not null)
        {
            // Nested types are not supported for interface generation.
            return null;
        }

        if (!GeneratedInterfaceNamingResolver.TryResolve(typeSymbol, context.Attributes[0], out var naming) || naming is null)
        {
            return null;
        }

        var resolvedNaming = naming;
        var interfaceName = StripTypeParameters(resolvedNaming.InterfaceName);
        var members = CollectMembers(typeSymbol, ct);
        var accessibility = ResolveAccessibility(typeSymbol.DeclaredAccessibility);

        // Extract class information for generating partial class implementation
        var className = typeSymbol.Name;
        var classNamespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var classTypeParameters = BuildTypeParametersString(typeSymbol);
        var typeParameterConstraints = BuildTypeParameterConstraintsString(typeSymbol);
        var typeParameterCount = typeSymbol.TypeParameters.Length;

        // TODO: Add diagnostic when class is not marked as partial

        return new GeneratedInterfaceInfo(
            interfaceName,
            resolvedNaming.InterfaceNamespace,
            accessibility,
            members,
            className,
            classNamespace,
            classTypeParameters,
            typeParameterCount,
            typeParameterConstraints);
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
                case IMethodSymbol method:
                    AddMemberIfNeeded(method, membersToSkip, membersBuilder, seenMembers);
                    break;
                case IPropertySymbol property when ShouldIgnoreProperty(property):
                    continue;
                case IPropertySymbol property:
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

        if (method.ReturnsByRef || method.ReturnsByRefReadonly)
        {
            return true;
        }

        if (HasRefLikeParameters(method.Parameters))
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

    private static bool ShouldIgnoreProperty(IPropertySymbol property)
    {
        if (property.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        if (property.ReturnsByRef || property.ReturnsByRefReadonly)
        {
            return true;
        }

        return HasRefLikeParameters(property.Parameters);
    }

    private static bool HasRefLikeParameters(ImmutableArray<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.RefKind != RefKind.None)
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveAccessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            _ => "internal"
        };
    }

    private static string BuildTypeParametersString(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeParameters.IsDefaultOrEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.Append('<');

        for (var i = 0; i < typeSymbol.TypeParameters.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(typeSymbol.TypeParameters[i].Name);
        }

        builder.Append('>');
        return builder.ToString();
    }

    private static string BuildTypeParameterConstraintsString(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeParameters.IsDefaultOrEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var typeParameter in typeSymbol.TypeParameters)
        {
            var constraints = new List<string>();

            if (typeParameter.HasReferenceTypeConstraint)
            {
                constraints.Add(typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");
            }

            if (typeParameter.HasUnmanagedTypeConstraint)
            {
                constraints.Add("unmanaged");
            }

            if (typeParameter.HasValueTypeConstraint)
            {
                constraints.Add("struct");
            }

            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                constraints.Add(constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            if (typeParameter.HasNotNullConstraint)
            {
                constraints.Add("notnull");
            }

            if (typeParameter.HasConstructorConstraint)
            {
                constraints.Add("new()");
            }

            if (constraints.Count == 0)
            {
                continue;
            }

            builder.Append(" where ")
                .Append(typeParameter.Name)
                .Append(" : ")
                .Append(string.Join(", ", constraints));
        }

        return builder.ToString();
    }

    private static string StripTypeParameters(string interfaceName)
    {
        var genericMarkerIndex = interfaceName.IndexOf('<');
        return genericMarkerIndex < 0
            ? interfaceName
            : interfaceName[..genericMarkerIndex];
    }
}
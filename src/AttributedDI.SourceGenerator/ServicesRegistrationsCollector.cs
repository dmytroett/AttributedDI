using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Collects attribute-based service registrations from the compilation.
/// </summary>
internal static class ServicesRegistrationsCollector
{
    /// <summary>
    /// Collects service registrations from the assembly.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental values provider of registration information.</returns>
    public static IncrementalValuesProvider<TypeWithAttributesInfo> Collect(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: CollectRegistrations)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);
    }

    /// <summary>
    /// Collects all service registrations from types with registration or lifetime attributes.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of registration information, or null if the type should be skipped.</returns>
    private static TypeWithAttributesInfo? CollectRegistrations(GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken: cancellationToken) is not
            INamedTypeSymbol symbol)
            return null;

        var allAttributes = symbol.GetAttributes();

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();

        // Find registration attributes
        var registrationAttributes = allAttributes
            .Where(attr =>
            {
                if (attr.AttributeClass == null)
                {
                    return false;
                }

                var attributeName = attr.AttributeClass.ToDisplayString();
                return
                    attributeName == KnownAttributes.RegisterAsSelfAttribute ||
                    attr.AttributeClass.Name == KnownAttributes.RegisterAsAttribute ||
                    attributeName ==
                    KnownAttributes.RegisterAsImplementedInterfacesAttribute;
            })
            .ToList();

        // Check if type has any lifetime attributes
        var lifetimeAttributes = allAttributes
            .Where(attr =>
            {
                if (attr.AttributeClass == null)
                {
                    return false;
                }

                var displayString = attr.AttributeClass.ToDisplayString();
                return
                    displayString == KnownAttributes.TransientAttribute ||
                    displayString == KnownAttributes.SingletonAttribute ||
                    displayString == KnownAttributes.ScopedAttribute;
            })
            .ToList();

        // TODO: Add diagnostic reporting for invalid registrations
        if (lifetimeAttributes.Count > 1)
        {
            // Multiple lifetime attributes found - skip this type
            return null;
        }

        var lifetime = ResolveLifetime(lifetimeAttributes);

        // If lifetime attribute is present without any registration attributes, register as self
        if (lifetimeAttributes.Count == 1 && registrationAttributes.Count == 0)
        {
            registrations.Add(new RegistrationInfo(
                symbol,
                RegistrationType.RegisterAsSelf,
                null,
                lifetime,
                null));
        }

        // Process registration attributes
        foreach (var attr in registrationAttributes)
        {
            var attrTypeName = attr.AttributeClass!.ToDisplayString();

            if (attrTypeName == KnownAttributes.RegisterAsSelfAttribute)
            {
                var key = ExtractKeyFromAttribute(attr);
                registrations.Add(new RegistrationInfo(
                    symbol,
                    RegistrationType.RegisterAsSelf,
                    null,
                    lifetime,
                    key));
            }
            else if (attr.AttributeClass.Name == KnownAttributes.RegisterAsAttribute)
            {
                // Handle generic RegisterAsAttribute<TService>
                if (attr.AttributeClass is { TypeArguments.Length: > 0 } namedAttr)
                {
                    var serviceType = namedAttr.TypeArguments[0];
                    var key = ExtractKeyFromAttribute(attr);
                    registrations.Add(new RegistrationInfo(
                        symbol,
                        RegistrationType.RegisterAs,
                        serviceType,
                        lifetime,
                        key));
                }
            }
            else if (attrTypeName == KnownAttributes.RegisterAsImplementedInterfacesAttribute)
            {
                var key = ExtractKeyFromAttribute(attr);
                registrations.Add(new RegistrationInfo(
                    symbol,
                    RegistrationType.RegisterAsImplementedInterfaces,
                    null,
                    lifetime,
                    key));
            }
        }

        if (registrations.Count == 0)
            return null;

        return new TypeWithAttributesInfo(symbol, registrations.ToImmutable());
    }

    private static string ResolveLifetime(List<AttributeData> lifetimeAttributes)
    {
        // No lifetime attribute - default to Transient
        if (lifetimeAttributes.Count == 0)
        {
            return "Transient";
        }

        // Single lifetime attribute found
        var lifetimeAttr = lifetimeAttributes[0];
        var lifetimeTypeName = lifetimeAttr.AttributeClass!.ToDisplayString();

        if (lifetimeTypeName == KnownAttributes.TransientAttribute)
            return "Transient";
        if (lifetimeTypeName == KnownAttributes.SingletonAttribute)
            return "Singleton";
        if (lifetimeTypeName == KnownAttributes.ScopedAttribute)
            return "Scoped";

        return "Transient";
    }

    /// <summary>
    /// Extracts the service key from an attribute's constructor arguments or named properties.
    /// </summary>
    /// <param name="attr">The attribute data to extract the key from.</param>
    /// <returns>The key value, or null if no key is specified.</returns>
    private static object? ExtractKeyFromAttribute(AttributeData attr)
    {
        // Check constructor arguments first (positional parameter)
        if (attr.ConstructorArguments.Length > 0)
        {
            var keyArg = attr.ConstructorArguments[0];
            if (!keyArg.IsNull)
            {
                return keyArg.Value;
            }
        }

        // Check named arguments (property assignment)
        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Key == "Key" && !namedArg.Value.IsNull)
            {
                return namedArg.Value.Value;
            }
        }

        return null;
    }
}

internal enum RegistrationType
{
    RegisterAsSelf,
    RegisterAs,
    RegisterAsImplementedInterfaces
}

internal sealed record RegistrationInfo(
    INamedTypeSymbol TypeSymbol,
    RegistrationType RegistrationType,
    ITypeSymbol? ServiceType,
    string Lifetime,
    object? Key);

internal sealed record TypeWithAttributesInfo(
    INamedTypeSymbol TypeSymbol,
    ImmutableArray<RegistrationInfo> Registrations);
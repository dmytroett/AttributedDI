using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace AttributedDI.SourceGenerator.Strategies;

/// <summary>
/// Strategy for collecting and generating attribute-based service registrations.
/// </summary>
internal static class ServiceRegistrationStrategy
{
    /// <summary>
    /// Collects all service registrations from types with registration or lifetime attributes.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of registration information, or null if the type should be skipped.</returns>
    public static TypeWithAttributesInfo? CollectRegistrations(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken: cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        var allAttributes = symbol.GetAttributes();

        // Determine lifetime
        var lifetime = LifetimeResolver.ResolveLifetime(allAttributes);

        // TODO: Add diagnostic reporting for invalid registrations
        if (lifetime == null)
        {
            // Multiple lifetime attributes found - skip this type
            return null;
        }

        // Find registration attributes
        var registrationAttributes = allAttributes
            .Where(attr => attr.AttributeClass != null &&
                          (attr.AttributeClass.ToDisplayString() == KnownAttributes.RegisterAsSelfAttribute ||
                           attr.AttributeClass.Name == KnownAttributes.RegisterAsAttribute ||
                           attr.AttributeClass.ToDisplayString() == KnownAttributes.RegisterAsImplementedInterfacesAttribute))
            .ToList();

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();

        // Check if type has any lifetime attributes
        var hasLifetimeAttribute = allAttributes.Any(attr =>
            attr.AttributeClass != null &&
            (attr.AttributeClass.ToDisplayString() == KnownAttributes.TransientAttribute ||
             attr.AttributeClass.ToDisplayString() == KnownAttributes.SingletonAttribute ||
             attr.AttributeClass.ToDisplayString() == KnownAttributes.ScopedAttribute));

        // If lifetime attribute is present without any registration attributes, register as self
        if (hasLifetimeAttribute && registrationAttributes.Count == 0)
        {
            registrations.Add(new RegistrationInfo(
                symbol,
                RegistrationType.RegisterAsSelf,
                null,
                lifetime));
        }

        // Process registration attributes
        foreach (var attr in registrationAttributes)
        {
            var attrTypeName = attr.AttributeClass!.ToDisplayString();

            if (attrTypeName == KnownAttributes.RegisterAsSelfAttribute)
            {
                registrations.Add(new RegistrationInfo(
                    symbol,
                    RegistrationType.RegisterAsSelf,
                    null,
                    lifetime));
            }
            else if (attr.AttributeClass.Name == KnownAttributes.RegisterAsAttribute)
            {
                // Handle generic RegisterAsAttribute<TService>
                if (attr.AttributeClass is { TypeArguments.Length: > 0 } namedAttr)
                {
                    var serviceType = namedAttr.TypeArguments[0];
                    registrations.Add(new RegistrationInfo(
                        symbol,
                        RegistrationType.RegisterAs,
                        serviceType,
                        lifetime));
                }
            }
            else if (attrTypeName == KnownAttributes.RegisterAsImplementedInterfacesAttribute)
            {
                registrations.Add(new RegistrationInfo(
                    symbol,
                    RegistrationType.RegisterAsImplementedInterfaces,
                    null,
                    lifetime));
            }
        }

        if (registrations.Count == 0)
            return null;

        return new TypeWithAttributesInfo(symbol, registrations.ToImmutable());
    }

    /// <summary>
    /// Generates service registration code.
    /// </summary>
    /// <param name="sb">The string builder to append code to.</param>
    /// <param name="registrations">The registrations to generate code for.</param>
    public static void GenerateCode(StringBuilder sb, ImmutableArray<RegistrationInfo> registrations)
    {
        foreach (var registration in registrations)
        {
            GenerateRegistrationCode(sb, registration);
        }
    }

    private static void GenerateRegistrationCode(StringBuilder sb, RegistrationInfo registration)
    {
        var typeSymbol = registration.TypeSymbol;
        string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string lifetime = registration.Lifetime;

        switch (registration.RegistrationType)
        {
            case RegistrationType.RegisterAsSelf:
                _ = sb.AppendLine($"            services.Add{lifetime}<{fullTypeName}>();");
                break;

            case RegistrationType.RegisterAs:
                if (registration.ServiceType != null)
                {
                    string serviceFullName = registration.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    _ = sb.AppendLine($"            services.Add{lifetime}<{serviceFullName}, {fullTypeName}>();");
                }

                break;

            case RegistrationType.RegisterAsImplementedInterfaces:
                var interfaces = typeSymbol.AllInterfaces;
                foreach (var iface in interfaces)
                {
                    string ifaceFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    _ = sb.AppendLine($"            services.Add{lifetime}<{ifaceFullName}, {fullTypeName}>();");
                }

                break;
        }
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
    string Lifetime);

internal sealed record TypeWithAttributesInfo(
    INamedTypeSymbol TypeSymbol,
    ImmutableArray<RegistrationInfo> Registrations);
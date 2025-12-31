using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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
    /// Scans the assembly for service registrations.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental values provider of registration information.</returns>
    public static IncrementalValuesProvider<TypeWithAttributesInfo> ScanAssembly(
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
        bool isKeyed = registration.Key != null;

        switch (registration.RegistrationType)
        {
            case RegistrationType.RegisterAsSelf:
                if (isKeyed)
                {
                    string keyLiteral = FormatKeyLiteral(registration.Key);
                    _ = sb.AppendLine($"            services.AddKeyed{lifetime}<{fullTypeName}>({keyLiteral});");
                }
                else
                {
                    _ = sb.AppendLine($"            services.Add{lifetime}<{fullTypeName}>();");
                }

                break;

            case RegistrationType.RegisterAs:
                if (registration.ServiceType != null)
                {
                    string serviceFullName =
                        registration.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (isKeyed)
                    {
                        string keyLiteral = FormatKeyLiteral(registration.Key);
                        _ = sb.AppendLine($"            services.AddKeyed{lifetime}<{serviceFullName}, {fullTypeName}>({keyLiteral});");
                    }
                    else
                    {
                        _ = sb.AppendLine($"            services.Add{lifetime}<{serviceFullName}, {fullTypeName}>();");
                    }
                }

                break;

            case RegistrationType.RegisterAsImplementedInterfaces:
                var interfaces = typeSymbol.AllInterfaces
                    .Where(static iface => !ImplementsDisposableContract(iface))
                    .Distinct(SymbolEqualityComparer.Default);
                foreach (var @interface in interfaces)
                {
                    if (@interface is null)
                    {
                        continue;
                    }

                    var interfaceFullName = @interface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (isKeyed)
                    {
                        string keyLiteral = FormatKeyLiteral(registration.Key);
                        _ = sb.AppendLine($"            services.AddKeyed{lifetime}<{interfaceFullName}, {fullTypeName}>({keyLiteral});");
                    }
                    else
                    {
                        _ = sb.AppendLine($"            services.Add{lifetime}<{interfaceFullName}, {fullTypeName}>();");
                    }
                }

                break;
        }
    }

    private static bool ImplementsDisposableContract(ITypeSymbol interfaceSymbol)
    {
        return IsDisposableInterface(interfaceSymbol) || interfaceSymbol.AllInterfaces.Any(IsDisposableInterface);
    }

    private static bool IsDisposableInterface(ITypeSymbol interfaceSymbol)
    {
        if (interfaceSymbol.SpecialType == SpecialType.System_IDisposable)
        {
            return true;
        }

        return interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IAsyncDisposable";
    }

    /// <summary>
    /// Formats a key value as a C# literal for code generation.
    /// </summary>
    /// <param name="key">The key value to format.</param>
    /// <returns>A string representation of the key suitable for code generation.</returns>
    private static string FormatKeyLiteral(object? key)
    {
        return key switch
        {
            null => "null",
            string s => $"\"{s.Replace("\"", "\\\"")}\"",
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => $"{l}L",
            bool b => b ? "true" : "false",
            _ => $"\"{key}\""
        };
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
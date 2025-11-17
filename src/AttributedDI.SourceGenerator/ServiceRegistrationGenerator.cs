using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     methods.
/// </summary>
[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    private const string RegisterAsSelfAttributeName = "AttributedDI.RegisterAsSelfAttribute";
    private const string RegisterAsImplementedInterfacesAttributeName = "AttributedDI.RegisterAsImplementedInterfacesAttribute";
    private const string RegistrationAliasAttributeName = "AttributedDI.RegistrationAliasAttribute";
    private const string TransientAttributeName = "AttributedDI.TransientAttribute";
    private const string SingletonAttributeName = "AttributedDI.SingletonAttribute";
    private const string ScopedAttributeName = "AttributedDI.ScopedAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all types with registration or lifetime attributes
        var typesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: GetTypeWithAttributes)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Detect RegistrationAlias attributes at assembly level
        var assemblyAliases = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RegistrationAliasAttributeName,
                static (_, _) => true,
                static (ctx, _) => GetAssemblyAliasInfo(ctx))
            .Where(static info => info is not null);

        // Combine with assembly aliases
        var combinedData = typesWithAttributes.Collect().Combine(assemblyAliases.Collect());

        // Generate the registration extension methods
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var typeInfos = data.Left;
            var aliases = data.Right.Where(a => a is not null).Select(a => a!).ToImmutableArray();
            
            // Flatten all registrations
            var allRegistrations = typeInfos.SelectMany(t => t.Registrations).ToImmutableArray();
            
            if (allRegistrations.Any())
            {
                string source = GenerateRegistrationExtensions(allRegistrations, aliases);
                spc.AddSource("ServiceRegistrationExtensions.g.cs", source);
            }
        });
    }

    private static TypeWithAttributesInfo? GetTypeWithAttributes(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var typeDeclaration = context.Node as TypeDeclarationSyntax;
        if (typeDeclaration == null)
            return null;

        var symbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
        if (symbol == null)
            return null;

        var allAttributes = symbol.GetAttributes();
        
        // Find lifetime attributes
        var lifetimeAttributes = allAttributes
            .Where(attr => attr.AttributeClass != null && 
                          (attr.AttributeClass.ToDisplayString() == TransientAttributeName ||
                           attr.AttributeClass.ToDisplayString() == SingletonAttributeName ||
                           attr.AttributeClass.ToDisplayString() == ScopedAttributeName))
            .ToList();

        // Validate only one lifetime attribute
        if (lifetimeAttributes.Count > 1)
        {
            // Report diagnostic for multiple lifetime attributes
            return null;
        }

        // Determine lifetime (default to Transient)
        string lifetime = "Transient";
        if (lifetimeAttributes.Count == 1)
        {
            var lifetimeAttr = lifetimeAttributes[0];
            var lifetimeTypeName = lifetimeAttr.AttributeClass!.ToDisplayString();
            
            if (lifetimeTypeName == TransientAttributeName)
                lifetime = "Transient";
            else if (lifetimeTypeName == SingletonAttributeName)
                lifetime = "Singleton";
            else if (lifetimeTypeName == ScopedAttributeName)
                lifetime = "Scoped";
        }

        // Find registration attributes
        var registrationAttributes = allAttributes
            .Where(attr => attr.AttributeClass != null && 
                          (attr.AttributeClass.ToDisplayString() == RegisterAsSelfAttributeName ||
                           attr.AttributeClass.Name == "RegisterAsAttribute" || // Generic attribute
                           attr.AttributeClass.ToDisplayString() == RegisterAsImplementedInterfacesAttributeName))
            .ToList();

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();

        // If lifetime attribute is present without any registration attributes, register as self
        if (lifetimeAttributes.Count > 0 && registrationAttributes.Count == 0)
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
            
            if (attrTypeName == RegisterAsSelfAttributeName)
            {
                registrations.Add(new RegistrationInfo(
                    symbol,
                    RegistrationType.RegisterAsSelf,
                    null,
                    lifetime));
            }
            else if (attr.AttributeClass.Name == "RegisterAsAttribute")
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
            else if (attrTypeName == RegisterAsImplementedInterfacesAttributeName)
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

    private static AssemblyAliasInfo? GetAssemblyAliasInfo(GeneratorAttributeSyntaxContext context)
    {
        var attribute = context.Attributes[0];

        // Get alias name (first constructor argument)
        if (attribute.ConstructorArguments.Length < 1)
        {
            return null;
        }

        var aliasArg = attribute.ConstructorArguments[0];
        if (aliasArg.Value is not string alias || string.IsNullOrWhiteSpace(alias))
        {
            return null;
        }

        // For assembly-level attributes, the TargetSymbol is the assembly itself
        var assemblySymbol = context.TargetSymbol as IAssemblySymbol;
        if (assemblySymbol is null)
        {
            // Fallback: if it's not directly an assembly, try getting the containing assembly
            assemblySymbol = context.TargetSymbol.ContainingAssembly;
        }
        
        if (assemblySymbol is null)
        {
            return null;
        }

        return new AssemblyAliasInfo(alias, assemblySymbol.Name);
    }


    private static string GenerateRegistrationExtensions(ImmutableArray<RegistrationInfo> registrations,
        ImmutableArray<AssemblyAliasInfo> aliases)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("namespace AttributedDI");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Extension methods for registering services marked with AttributedDI attributes.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static partial class RegistrationServiceCollectionExtensions");
        sb.AppendLine("    {");

        // Generate a single registration method for this assembly
        if (registrations.Any())
        {
            string assemblyName = registrations[0].TypeSymbol.ContainingAssembly.Name;

            // Check if there's an alias for this assembly
            var aliasInfo = aliases.FirstOrDefault(a => a.AssemblyName == assemblyName);
            string methodName = aliasInfo != null
                ? $"Add{SanitizeIdentifier(aliasInfo.Alias)}"
                : $"Add{SanitizeIdentifier(assemblyName)}";

            sb.AppendLine("        /// <summary>");
            sb.AppendLine(
                $"        /// Registers all services from the {assemblyName} assembly that are marked with registration attributes.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"services\">The service collection to add services to.</param>");
            sb.AppendLine("        /// <returns>The service collection for chaining.</returns>");
            sb.AppendLine($"        public static IServiceCollection {methodName}(this IServiceCollection services)");
            sb.AppendLine("        {");

            foreach (var registration in registrations)
            {
                GenerateRegistrationCode(sb, registration);
            }

            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string SanitizeIdentifier(string name)
    {
        // Remove invalid characters from name to create a valid C# identifier
        string sanitized = new(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Ensure it doesn't start with a digit
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }

    private static void GenerateRegistrationCode(StringBuilder sb, RegistrationInfo registration)
    {
        var typeSymbol = registration.TypeSymbol;
        string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string lifetime = registration.Lifetime;

        switch (registration.RegistrationType)
        {
            case RegistrationType.RegisterAsSelf:
                sb.AppendLine($"            services.Add{lifetime}<{fullTypeName}>();");
                break;

            case RegistrationType.RegisterAs:
                if (registration.ServiceType != null)
                {
                    string serviceFullName = registration.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    sb.AppendLine($"            services.Add{lifetime}<{serviceFullName}, {fullTypeName}>();");
                }
                break;

            case RegistrationType.RegisterAsImplementedInterfaces:
                var interfaces = typeSymbol.AllInterfaces;
                foreach (var iface in interfaces)
                {
                    string ifaceFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    sb.AppendLine($"            services.Add{lifetime}<{ifaceFullName}, {fullTypeName}>();");
                }
                break;
        }
    }

    private enum RegistrationType
    {
        RegisterAsSelf,
        RegisterAs,
        RegisterAsImplementedInterfaces
    }

    private sealed record RegistrationInfo(
        INamedTypeSymbol TypeSymbol,
        RegistrationType RegistrationType,
        ITypeSymbol? ServiceType,
        string Lifetime);

    private sealed record TypeWithAttributesInfo(
        INamedTypeSymbol TypeSymbol,
        ImmutableArray<RegistrationInfo> Registrations);

    private sealed record AssemblyAliasInfo(
        string Alias,
        string AssemblyName);
}
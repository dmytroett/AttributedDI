using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     methods.
/// </summary>
[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    private const string RegisterAsSelfAttributeName = "AttributedDI.RegisterAsSelfAttribute";
    private const string RegisterAsAttributeName = "AttributedDI.RegisterAsAttribute";

    private const string RegisterAsImplementedInterfacesAttributeName =
        "AttributedDI.RegisterAsImplementedInterfacesAttribute";

    private const string RegistrationAliasAttributeName = "AttributedDI.RegistrationAliasAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute for RegisterAsSelfAttribute
        var registerAsSelfTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RegisterAsSelfAttributeName,
                static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                static (ctx, _) => GetRegistrationInfo(ctx))
            .SelectMany(static (infos, _) => infos);

        // Register attribute for RegisterAsAttribute
        var registerAsTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RegisterAsAttributeName,
                static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                static (ctx, _) => GetRegistrationInfo(ctx))
            .SelectMany(static (infos, _) => infos);

        // Register attribute for RegisterAsImplementedInterfacesAttribute
        var registerAsInterfacesTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RegisterAsImplementedInterfacesAttributeName,
                static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                static (ctx, _) => GetRegistrationInfo(ctx))
            .SelectMany(static (infos, _) => infos);

        // Detect RegistrationAlias attributes at assembly level
        var assemblyAliases = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                RegistrationAliasAttributeName,
                static (_, _) => true,
                static (ctx, _) => GetAssemblyAliasInfo(ctx))
            .Where(static info => info is not null);

        // Combine all registration types
        var allRegistrations = registerAsSelfTypes
            .Collect()
            .Combine(registerAsTypes.Collect())
            .Combine(registerAsInterfacesTypes.Collect())
            .Select(static (combined, _) =>
            {
                var left = combined.Left;
                var selfTypes = left.Left;
                var asTypes = left.Right;
                var interfaceTypes = combined.Right;

                return selfTypes.Concat(asTypes).Concat(interfaceTypes).ToImmutableArray();
            });

        // Combine registrations with assembly aliases
        var combinedData = allRegistrations.Combine(assemblyAliases.Collect());

        // Generate the registration extension methods
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var registrations = data.Left;
            var aliases = data.Right.Where(a => a is not null).Select(a => a!).ToImmutableArray();
            string source = GenerateRegistrationExtensions(registrations, aliases);
            spc.AddSource("ServiceRegistrationExtensions.g.cs", source);
        });
    }

    private static ImmutableArray<RegistrationInfo> GetRegistrationInfo(GeneratorAttributeSyntaxContext context)
    {
        var symbol = context.TargetSymbol as INamedTypeSymbol;
        if (symbol is null)
        {
            return ImmutableArray<RegistrationInfo>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<RegistrationInfo>();
        
        // Process ALL attributes, not just the first one
        foreach (var attribute in context.Attributes)
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
                continue;

            builder.Add(new RegistrationInfo(
                symbol,
                attribute,
                attributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        }

        return builder.ToImmutable();
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

        // Get the assembly symbol from the context
        var assemblySymbol = context.TargetSymbol.ContainingAssembly;
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
        var attribute = registration.Attribute;
        string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Get the lifetime from the attribute
        string lifetime = "Transient";
        var lifetimeArg = attribute.NamedArguments.FirstOrDefault(a => a.Key == "Lifetime").Value;
        if (lifetimeArg.Value != null)
        {
            // Extract enum member name (e.g., "Transient" from "ServiceLifetime.Transient" or "2")
            string lifetimeStr = lifetimeArg.Value.ToString()!;
            lifetime = lifetimeStr.Contains('.') ? lifetimeStr.Split('.').Last() : lifetimeStr;
        }
        else if (attribute.ConstructorArguments.Length > 0)
        {
            var lastArg = attribute.ConstructorArguments[attribute.ConstructorArguments.Length - 1];
            if (lastArg.Type?.Name == "ServiceLifetime")
            {
                // For enum values, we need to convert from integer to name
                // ServiceLifetime: Singleton = 0, Scoped = 1, Transient = 2
                object? lifetimeValue = lastArg.Value;
                if (lifetimeValue is int intValue)
                {
                    lifetime = intValue switch
                    {
                        0 => "Singleton",
                        1 => "Scoped",
                        _ => "Transient"
                    };
                }
                else if (lifetimeValue != null)
                {
                    string lifetimeStr = lifetimeValue.ToString()!;
                    lifetime = lifetimeStr.Contains('.') ? lifetimeStr.Split('.').Last() : lifetimeStr;
                }
            }
        }

        string attributeTypeName = registration.AttributeTypeName;

        if (attributeTypeName.Contains("RegisterAsSelfAttribute"))
        {
            // services.AddXXX<TypeName>();
            sb.AppendLine($"            services.Add{lifetime}<{fullTypeName}>();");
        }
        else if (attributeTypeName.Contains("RegisterAsImplementedInterfacesAttribute"))
        {
            // Register for each implemented interface
            var interfaces = typeSymbol.AllInterfaces;
            foreach (var iface in interfaces)
            {
                string ifaceFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"            services.Add{lifetime}<{ifaceFullName}, {fullTypeName}>();");
            }
        }
        else if (attributeTypeName.Contains("RegisterAsAttribute"))
        {
            // Get the service type from the attribute constructor
            if (attribute.ConstructorArguments.Length > 0)
            {
                var serviceTypeArg = attribute.ConstructorArguments[0];
                if (serviceTypeArg.Value is INamedTypeSymbol serviceType)
                {
                    string serviceFullName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    sb.AppendLine($"            services.Add{lifetime}<{serviceFullName}, {fullTypeName}>();");
                }
            }
        }
    }

    private sealed record RegistrationInfo(
        INamedTypeSymbol TypeSymbol,
        AttributeData Attribute,
        string AttributeTypeName);

    private sealed record AssemblyAliasInfo(
        string Alias,
        string AssemblyName);
}
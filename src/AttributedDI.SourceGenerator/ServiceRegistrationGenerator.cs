using AttributedDI.SourceGenerator.Strategies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     methods.
/// </summary>
[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all types with registration or lifetime attributes
        var typesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: ServiceRegistrationStrategy.CollectRegistrations)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Collect all modules with RegisterModule attribute
        var modules = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: ModuleRegistrationStrategy.CollectModules)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Detect RegistrationMethodName attributes at assembly level
        var assemblyAliases = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                KnownAttributes.RegistrationMethodNameAttribute,
                static (_, _) => true,
                static (ctx, _) => RegistrationMethodNameResolver.GetAssemblyAliasInfo(ctx))
            .Where(static info => info is not null);

        // Combine all data sources
        var combinedData = typesWithAttributes.Collect()
            .Combine(modules.Collect())
            .Combine(assemblyAliases.Collect());

        // Generate the registration extension methods
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var typeInfos = data.Left.Left;
            var moduleInfos = data.Left.Right;
            var aliases = data.Right.Where(a => a is not null).Select(a => a!).ToImmutableArray();

            // Flatten all registrations
            var allRegistrations = typeInfos.SelectMany(t => t.Registrations).ToImmutableArray();

            if (allRegistrations.Any() || moduleInfos.Any())
            {
                string assemblyName = allRegistrations.Any()
                    ? allRegistrations[0].TypeSymbol.ContainingAssembly.Name
                    : moduleInfos[0].TypeSymbol.ContainingAssembly.Name;

                var aliasInfo = aliases.FirstOrDefault(a => a.AssemblyName == assemblyName);
                string methodName = RegistrationMethodNameResolver.Resolve(assemblyName, aliasInfo);

                string source = CodeEmitter.EmitRegistrationExtension(
                    methodName,
                    assemblyName,
                    allRegistrations,
                    moduleInfos);

                spc.AddSource("ServiceRegistrationExtensions.g.cs", source);
            }
        });
    }
}
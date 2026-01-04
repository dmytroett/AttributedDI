using AttributedDI.SourceGenerator.InterfacesGeneration;
using AttributedDI.SourceGenerator.ServiceModulesGeneration;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     modules.
/// </summary>
[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Phase 1: locate the attributes & extract structured info for code generation.
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.Assembly.Name);
        var isEntryPoint = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.OutputKind != OutputKind.DynamicallyLinkedLibrary);
        var registrations = ServicesRegistrationsCollector.Collect(context);
        var customModuleNameInfo = GeneratedModuleNameCollector.Collect(context);
        var referencedModules = GeneratedModuleReferenceCollector.Collect(context);
        var generatedInterfaces = InterfaceGenerationPipeline.Collect(context);

        var combinedData = registrations.Collect()
            .Combine(customModuleNameInfo)
            .Combine(assemblyName);
        var moduleRegistrations = combinedData
            .Select(static (data, _) =>
            {
                var ((registrationInfos, customNameInfo), _) = data;

                if (registrationInfos.IsDefaultOrEmpty)
                {
                    return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
                }

                var fullyQualifiedName = $"global::{customNameInfo.Namespace}.{customNameInfo.ModuleName}";
                return ImmutableArray.Create(new GeneratedModuleRegistrationInfo(fullyQualifiedName));
            })
            .Combine(referencedModules)
            .Select(static (data, _) =>
            {
                var currentModules = data.Left;
                var referenced = data.Right;

                if (currentModules.IsDefaultOrEmpty && referenced.IsDefaultOrEmpty)
                {
                    return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
                }

                var combined = currentModules.IsDefaultOrEmpty
                    ? referenced
                    : referenced.IsDefaultOrEmpty
                        ? currentModules
                        : currentModules.AddRange(referenced);

                if (combined.Length <= 1)
                {
                    return combined;
                }

                return combined
                    .Distinct()
                    .OrderBy(static info => info.FullyQualifiedTypeName, StringComparer.Ordinal)
                    .ToImmutableArray();
            })
            .Combine(isEntryPoint);

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var ((registrationInfos, customNameInfo), assemblyName) = data;

            if (registrationInfos.Any())
            {
                GeneratedModuleCodeEmitter.EmitRegistrationModule(
                    spc,
                    customNameInfo.ModuleName,
                    customNameInfo.MethodName,
                    customNameInfo.Namespace,
                    assemblyName,
                    registrationInfos);
            }
        });

        context.RegisterSourceOutput(moduleRegistrations, static (spc, data) =>
        {
            var modules = data.Left;
            var isEntryPoint = data.Right;

            if (!isEntryPoint || modules.IsDefaultOrEmpty)
            {
                return;
            }

            GeneratedModuleInitializerEmitter.EmitModuleInitializer(spc, modules);
        });

        context.RegisterSourceOutput(generatedInterfaces.Collect(), static (spc, interfaces) =>
        {
            if (interfaces.IsDefaultOrEmpty)
            {
                return;
            }

            GeneratedInterfacesCodeEmitter.EmitInterfaces(spc, interfaces);
        });
    }
}
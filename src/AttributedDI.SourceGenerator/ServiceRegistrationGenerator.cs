using AttributedDI.SourceGenerator.InterfacesGeneration;
using AttributedDI.SourceGenerator.ModuleInitializerGeneration;
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
        var moduleToGenerate = ModuleGenerationPipeline.Collect(context);
        var generatedInterfaces = InterfaceGenerationPipeline.Collect(context);
        var referencedModules = ModuleInitializerPipeline.Collect(context, moduleToGenerate);        

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(moduleToGenerate, static (spc, data) =>
        {
            var (registrationInfos, customNameInfo, assemblyName) = data;

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

        context.RegisterSourceOutput(referencedModules, static (spc, modules) =>
        {
            if (modules.IsDefaultOrEmpty)
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
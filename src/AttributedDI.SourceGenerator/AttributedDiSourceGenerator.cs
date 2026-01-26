using AttributedDI.SourceGenerator.AggregateServiceCollectionExtensionGeneration;
using AttributedDI.SourceGenerator.InterfacesGeneration;
using AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     modules.
/// </summary>
[Generator]
public class AttributedDiSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generates and embeds marker attributes used by the generator.
        GeneratedAttributesInitializer.EmbedGeneratedAttributes(context);

        // Phase 1: locate the attributes & extract structured info for code generation.        
        var extensionToGenerate = ServiceCollectionExtensionGenerationPipeline.Collect(context);
        var generatedInterfaces = InterfaceGenerationPipeline.Collect(context);
        var addAttributedDiExtensions = AttributedDiServiceCollectionExtensionsPipeline.Collect(context, extensionToGenerate);

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(extensionToGenerate, static (spc, data) =>
        {
            var (registrationInfos, extensionNames, assemblyName) = data;

            if (registrationInfos.Any())
            {
                ServiceCollectionExtensionCodeEmitter.EmitServiceCollectionExtension(
                    spc,
                    extensionNames.ExtensionClassName,
                    extensionNames.MethodName,
                    extensionNames.Namespace,
                    assemblyName,
                    registrationInfos);
            }
        });

        context.RegisterSourceOutput(addAttributedDiExtensions, static (spc, info) =>
        {
            if (!info.ShouldGenerateExtensions)
            {
                return;
            }

            AttributedDiServiceCollectionExtensionsEmitter.EmitExtensionMethod(spc, info.Extensions);
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
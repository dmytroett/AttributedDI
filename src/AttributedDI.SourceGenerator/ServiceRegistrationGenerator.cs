using Microsoft.CodeAnalysis;
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
        // Phase 1: Strategies discover and collect what they observe
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.Assembly.Name);
        var registrations = ServicesRegistrationsCollector.Collect(context);
        var customModuleNameInfo = GeneratedModuleNameCollector.Collect(context);
        var generatedInterfaces = GeneratedInterfacesCollector.Collect(context);

        // Combine all collected data with compilation provider for assembly name
        var combinedData = registrations.Collect()
            .Combine(customModuleNameInfo)
            .Combine(assemblyName);

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
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

internal static class ServiceCollectionExtensionGenerationPipeline
{
    public static IncrementalValueProvider<ServiceCollectionExtensionToGenerate> Collect(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.Assembly.Name);
        var registrations = ServicesRegistrationsCollector.Collect(context);
        var customExtensionNameInfo = ServiceCollectionExtensionNameCollector.Collect(context);

        var combinedData = registrations.Collect()
            .Combine(customExtensionNameInfo)
            .Combine(assemblyName)
            .Select(static (data, _) =>
            {
                var ((registrationInfos, customNameInfo), assemblyName) = data;

                return new ServiceCollectionExtensionToGenerate(registrationInfos, customNameInfo, assemblyName);
            });

        return combinedData;
    }
}

internal sealed record ServiceCollectionExtensionToGenerate(
    ImmutableArray<RegistrationInfo> Registrations,
    ResolvedExtensionNames ExtensionNames,
    string AssemblyName);

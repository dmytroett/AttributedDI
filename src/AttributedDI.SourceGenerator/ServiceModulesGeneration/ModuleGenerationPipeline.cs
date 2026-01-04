using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

internal static class ModuleGenerationPipeline
{
    public static IncrementalValueProvider<ServiceModuleToGenerate> Collect(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.Assembly.Name);
        var registrations = ServicesRegistrationsCollector.Collect(context);
        var customModuleNameInfo = GeneratedModuleNameCollector.Collect(context);

        var combinedData = registrations.Collect()
            .Combine(customModuleNameInfo)
            .Combine(assemblyName)
            .Select(static (data, _) =>
            {
                var ((registrationInfos, customNameInfo), assemblyName) = data;

                return new ServiceModuleToGenerate(registrationInfos, customNameInfo, assemblyName);
            });

        return combinedData;
    }
}

internal record ServiceModuleToGenerate(ImmutableArray<RegistrationInfo> Registrations, ResolvedModuleNames ModuleNames, string AssemblyName);
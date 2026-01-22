using AttributedDI.SourceGenerator.ServiceModulesGeneration;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionsGeneration;

internal static class AttributedDiServiceCollectionExtensionsPipeline
{
    private const string GenerateExtensionsPropertyName = "GenerateAttributedDIExtensions";

    public static IncrementalValueProvider<AttributedDiServiceCollectionExtensionsInfo> Collect(
        IncrementalGeneratorInitializationContext context, IncrementalValueProvider<ServiceModuleToGenerate> moduleToGenerate)
    {
        var shouldGenerateExtensionMethodProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) =>
            {
                // TODO: emit diagnostic that the value should be either true or false
                if (provider.GlobalOptions.TryGetValue($"build_property.{GenerateExtensionsPropertyName}", out var value)
                    && bool.TryParse(value, out var parsed))
                {
                    return parsed;
                }

                return false;
            });

        var currentProjectModule = moduleToGenerate
            .Combine(shouldGenerateExtensionMethodProvider)
            .Select(static (module, _) =>
            {
                var ((registrationInfos, customNameInfo, _), shouldGenerateExtensionMethod) = module;

                if (!shouldGenerateExtensionMethod || registrationInfos.IsDefaultOrEmpty)
                {
                    return null;
                }

                var fullyQualifiedName = $"global::{customNameInfo.Namespace}.{customNameInfo.ModuleName}";
                return (GeneratedModuleRegistrationInfo?)new GeneratedModuleRegistrationInfo(fullyQualifiedName);
            });

        var generatedModulesFromReferencesProvider = context.CompilationProvider
            .Combine(shouldGenerateExtensionMethodProvider)
            .Select(static (data, token) =>
            {
                var (compilation, shouldGenerateExtensionMethod) = data;

                if (!shouldGenerateExtensionMethod)
                {
                    return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
                }

                return GeneratedModuleReferenceCollector.CollectGeneratedModulesFromReferences(compilation, token);
            });

        return shouldGenerateExtensionMethodProvider
            .Combine(generatedModulesFromReferencesProvider)
            .Combine(currentProjectModule)
            .Select(static (data, _) =>
            {
                var ((shouldGenerateExtensionMethod, modulesFromReferences), currentModule) = data;

                if (!shouldGenerateExtensionMethod)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(
                        false,
                        ImmutableArray<GeneratedModuleRegistrationInfo>.Empty);
                }

                if (currentModule == null)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(true, modulesFromReferences);
                }

                return new AttributedDiServiceCollectionExtensionsInfo(
                    true,
                    modulesFromReferences.Add(currentModule));
            });
    }
}

internal sealed record GeneratedModuleRegistrationInfo(string FullyQualifiedTypeName);

internal sealed record AttributedDiServiceCollectionExtensionsInfo(
    bool ShouldGenerateExtensions,
    ImmutableArray<GeneratedModuleRegistrationInfo> ModuleTypes);
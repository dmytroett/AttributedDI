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
        var generateExtensionsOverride = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) =>
            {
                if (provider.GlobalOptions.TryGetValue($"build_property.{GenerateExtensionsPropertyName}", out var value)
                    && bool.TryParse(value, out var parsed))
                {
                    return (bool?)parsed;
                }

                return null;
            });

        var shouldGenerateExtensions = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.OutputKind)
            .Combine(generateExtensionsOverride)
            .Select(static (data, _) =>
            {
                var (outputKind, overrideValue) = data;

                return overrideValue ?? outputKind != OutputKind.DynamicallyLinkedLibrary;
            });

        var generatedModulesFromReferences = context.CompilationProvider
            .Combine(shouldGenerateExtensions)
            .Select(static (data, token) =>
            {
                var (compilation, shouldGenerate) = data;

                if (!shouldGenerate)
                {
                    return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
                }

                return GeneratedModuleReferenceCollector.CollectGeneratedModulesFromReferences(compilation, token);
            });

        var modulesToInclude = moduleToGenerate
            .Select(static (module, _) =>
            {
                var (registrationInfos, customNameInfo, _) = module;

                if (registrationInfos.IsDefaultOrEmpty)
                {
                    return null;
                }

                var fullyQualifiedName = $"global::{customNameInfo.Namespace}.{customNameInfo.ModuleName}";
                return (GeneratedModuleRegistrationInfo?)new GeneratedModuleRegistrationInfo(fullyQualifiedName);
            });

        return shouldGenerateExtensions
            .Combine(generatedModulesFromReferences)
            .Combine(modulesToInclude)
            .Select(static (data, _) =>
            {
                var ((shouldGenerateExtensions, modulesFromReferences), currentModuleToInclude) = data;

                if (!shouldGenerateExtensions)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(
                        false,
                        ImmutableArray<GeneratedModuleRegistrationInfo>.Empty);
                }

                if (currentModuleToInclude == null)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(true, modulesFromReferences);
                }

                return new AttributedDiServiceCollectionExtensionsInfo(
                    true,
                    modulesFromReferences.Add(currentModuleToInclude));
            });
    }
}

internal sealed record GeneratedModuleRegistrationInfo(string FullyQualifiedTypeName);

internal sealed record AttributedDiServiceCollectionExtensionsInfo(
    bool ShouldGenerateExtensions,
    ImmutableArray<GeneratedModuleRegistrationInfo> ModuleTypes);
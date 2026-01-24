using AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.AggregateServiceCollectionExtensionGeneration;

internal static class AttributedDiServiceCollectionExtensionsPipeline
{
    private const string GenerateExtensionsPropertyName = "GenerateAttributedDIExtensions";

    public static IncrementalValueProvider<AttributedDiServiceCollectionExtensionsInfo> Collect(
        IncrementalGeneratorInitializationContext context, IncrementalValueProvider<ServiceCollectionExtensionToGenerate> extensionToGenerate)
    {
        var shouldGenerateExtensionMethodProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) =>
            {
                if (provider.GlobalOptions.TryGetValue($"build_property.{GenerateExtensionsPropertyName}", out var value)
                    && bool.TryParse(value, out var parsed))
                {
                    return parsed;
                }

                return false;
            });

        var currentProjectExtension = extensionToGenerate
            .Combine(shouldGenerateExtensionMethodProvider)
            .Select(static (module, _) =>
            {
                var ((registrationInfos, customNameInfo, _), shouldGenerateExtensionMethod) = module;

                if (!shouldGenerateExtensionMethod || registrationInfos.IsDefaultOrEmpty)
                {
                    return null;
                }

                var fullyQualifiedName = BuildFullyQualifiedTypeName(customNameInfo.Namespace, customNameInfo.ExtensionClassName);
                return (ExportedServiceCollectionExtensionInfo?)new ExportedServiceCollectionExtensionInfo(
                    fullyQualifiedName,
                    customNameInfo.MethodName);
            });

        var exportedExtensionsFromReferencesProvider = context.CompilationProvider
            .Combine(shouldGenerateExtensionMethodProvider)
            .Select(static (data, token) =>
            {
                var (compilation, shouldGenerateExtensionMethod) = data;

                if (!shouldGenerateExtensionMethod)
                {
                    return ImmutableArray<ExportedServiceCollectionExtensionInfo>.Empty;
                }

                return ExportedServiceCollectionExtensionCollector.CollectFromReferences(compilation, token);
            });

        return shouldGenerateExtensionMethodProvider
            .Combine(exportedExtensionsFromReferencesProvider)
            .Combine(currentProjectExtension)
            .Select(static (data, _) =>
            {
                var ((shouldGenerateExtensionMethod, extensionsFromReferences), currentExtension) = data;

                if (!shouldGenerateExtensionMethod)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(
                        false,
                        ImmutableArray<ExportedServiceCollectionExtensionInfo>.Empty);
                }

                if (currentExtension == null)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(true, extensionsFromReferences);
                }

                return new AttributedDiServiceCollectionExtensionsInfo(
                    true,
                    extensionsFromReferences.Add(currentExtension));
            });
    }

    private static string BuildFullyQualifiedTypeName(string namespaceName, string className)
    {
        if (string.IsNullOrWhiteSpace(namespaceName) || namespaceName == "<global namespace>")
        {
            return $"global::{className}";
        }

        return $"global::{namespaceName}.{className}";
    }
}

internal sealed record ExportedServiceCollectionExtensionInfo(string FullyQualifiedTypeName, string MethodName);

internal sealed record AttributedDiServiceCollectionExtensionsInfo(
    bool ShouldGenerateExtensions,
    ImmutableArray<ExportedServiceCollectionExtensionInfo> Extensions);

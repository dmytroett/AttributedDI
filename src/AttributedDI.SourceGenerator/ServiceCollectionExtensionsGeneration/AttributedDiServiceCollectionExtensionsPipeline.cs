using AttributedDI.SourceGenerator.ServiceModulesGeneration;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionsGeneration;

internal static class AttributedDiServiceCollectionExtensionsPipeline
{
    public static IncrementalValueProvider<AttributedDiServiceCollectionExtensionsInfo> Collect(
        IncrementalGeneratorInitializationContext context, IncrementalValueProvider<ServiceModuleToGenerate> moduleToGenerate)
    {
        var isEntryPoint = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.OutputKind != OutputKind.DynamicallyLinkedLibrary);

        var generatedModulesFromReferences = context.CompilationProvider
            .Select(static (compilation, token) => CollectFromCompilation(compilation, token));

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

        return isEntryPoint
            .Combine(generatedModulesFromReferences)
            .Combine(modulesToInclude)
            .Select(static (data, _) =>
            {
                var ((isEntryPoint, modulesFromReferences), currentModuleToInclude) = data;

                if (!isEntryPoint)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(
                        false,
                        ImmutableArray<GeneratedModuleRegistrationInfo>.Empty);
                }

                if (currentModuleToInclude == null)
                {
                    return new AttributedDiServiceCollectionExtensionsInfo(true, modulesFromReferences);
                }

                return new AttributedDiServiceCollectionExtensionsInfo(true, modulesFromReferences.Add(currentModuleToInclude));
            });
    }

    private static ImmutableArray<GeneratedModuleRegistrationInfo> CollectFromCompilation(Compilation compilation, CancellationToken token)
    {
        // do not run the collection if current project is DLL.
        if (compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary)
        {
            return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
        }

        return GeneratedModuleReferenceCollector.CollectGeneratedModulesFromReferences(compilation, token);
    }
}

internal sealed record GeneratedModuleRegistrationInfo(string FullyQualifiedTypeName);

internal sealed record AttributedDiServiceCollectionExtensionsInfo(
    bool IsEntryPoint,
    ImmutableArray<GeneratedModuleRegistrationInfo> ModuleTypes);
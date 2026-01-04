using AttributedDI.SourceGenerator.ServiceModulesGeneration;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;

namespace AttributedDI.SourceGenerator.ModuleInitializerGeneration;

internal static class ModuleInitializerPipeline
{
    public static IncrementalValueProvider<ImmutableArray<GeneratedModuleRegistrationInfo>> Collect(
        IncrementalGeneratorInitializationContext context, IncrementalValueProvider<ServiceModuleToGenerate> moduleToGenerate)
    {
        var isEntryPoint = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Options.OutputKind != OutputKind.DynamicallyLinkedLibrary);

        var generatedModulesFromReferences = context.CompilationProvider
            .Select(static (compilation, token) => CollectFromCompilation(compilation, token));

        var modulesToInitialize = moduleToGenerate
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

        var result = isEntryPoint
            .Combine(generatedModulesFromReferences)
            .Combine(modulesToInitialize)
            .Select(static (data, _) =>
            {
                var ((isEntryPoint, modulesFromReferences), currentModuleToInitialize) = data;

                if (!isEntryPoint)
                {
                    return ImmutableArray<GeneratedModuleRegistrationInfo>.Empty;
                }

                if (!currentModuleToInitialize.HasValue)
                {
                    return modulesFromReferences;
                }

                return modulesFromReferences.Add(currentModuleToInitialize.Value);
            });

        return result;
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

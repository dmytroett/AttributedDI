using AttributedDI.SourceGenerator.Strategies;
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
        var typesWithAttributes = ServiceRegistrationStrategy.ScanAssembly(context);
        var assemblyInfo = GeneratedModuleNameResolver.ScanAssembly(context);

        // Combine all collected data
        var combinedData = typesWithAttributes.Collect()
            .Combine(assemblyInfo);

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var typeInfos = data.Left;
            var assemblyInfo = data.Right;

            var allRegistrations = typeInfos.SelectMany(t => t.Registrations).ToImmutableArray();

            if (allRegistrations.Any())
            {
                string moduleName = GeneratedModuleNameResolver.ResolveModuleName(assemblyInfo.AssemblyName, assemblyInfo);
                string methodName = GeneratedModuleNameResolver.ResolveMethodName(assemblyInfo.AssemblyName, assemblyInfo);

                CodeEmitter.EmitRegistrationModule(
                    spc,
                    moduleName,
                    methodName,
                    assemblyInfo.AssemblyName,
                    allRegistrations);
            }
        });
    }
}
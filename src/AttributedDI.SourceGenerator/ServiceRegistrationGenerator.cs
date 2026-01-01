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
        var customModuleNameInfo = GeneratedModuleNameCollector.Collect(context);

        // Combine all collected data with compilation provider for assembly name
        var combinedData = typesWithAttributes.Collect()
            .Combine(customModuleNameInfo)
            .Combine(context.CompilationProvider);

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var typeInfos = data.Left.Left;
            var customNameInfo = data.Left.Right;
            var compilation = data.Right;

            var allRegistrations = typeInfos.SelectMany(t => t.Registrations).ToImmutableArray();

            if (allRegistrations.Any())
            {
                string assemblyName = compilation.Assembly.Name;

                CodeEmitter.EmitRegistrationModule(
                    spc,
                    customNameInfo.ModuleName,
                    customNameInfo.MethodName,
                    assemblyName,
                    allRegistrations);
            }
        });
    }
}
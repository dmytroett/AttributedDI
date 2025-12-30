using AttributedDI.SourceGenerator.Strategies;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
///     Incremental source generator that discovers types with registration attributes and generates service registration
///     methods.
/// </summary>
[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Phase 1: Strategies discover and collect what they observe
        var typesWithAttributes = ServiceRegistrationStrategy.ScanAssembly(context);
        var modules = ModuleRegistrationStrategy.ScanAssembly(context);
        var assemblyInfo = RegistrationMethodNameResolver.ScanAssembly(context);

        // Combine all collected data
        var combinedData = typesWithAttributes.Collect()
            .Combine(modules.Collect())
            .Combine(assemblyInfo);

        // Phase 2: Generate code based on collected data
        context.RegisterSourceOutput(combinedData, static (spc, data) =>
        {
            var typeInfos = data.Left.Left;
            var moduleInfos = data.Left.Right;
            var assemblyInfo = data.Right;

            var allRegistrations = typeInfos.SelectMany(t => t.Registrations).ToImmutableArray();

            if (allRegistrations.Any() || moduleInfos.Any())
            {
                string methodName = RegistrationMethodNameResolver.Resolve(assemblyInfo.AssemblyName, assemblyInfo);

                string source = CodeEmitter.EmitRegistrationExtension(
                    methodName,
                    assemblyInfo.AssemblyName,
                    allRegistrations,
                    moduleInfos);

                spc.AddSource("ServiceRegistrationExtensions.g.cs", source);
            }
        });
    }
}
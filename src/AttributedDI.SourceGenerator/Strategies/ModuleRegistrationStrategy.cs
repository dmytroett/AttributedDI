using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace AttributedDI.SourceGenerator.Strategies;

/// <summary>
/// Strategy for collecting and generating module-based service registrations.
/// </summary>
internal static class ModuleRegistrationStrategy
{
    /// <summary>
    /// Collects all modules from types with RegisterModule attribute.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Module information, or null if the type should be skipped.</returns>
    public static ModuleInfo? CollectModules(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken: cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        // Check if the type has RegisterModule attribute
        var hasRegisterModuleAttribute = symbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.ToDisplayString() == KnownAttributes.RegisterModuleAttribute);

        if (!hasRegisterModuleAttribute)
            return null;

        // TODO: Add diagnostic reporting for modules not implementing IServiceModule
        // Check if the type implements IServiceModule
        var implementsIServiceModule = symbol.AllInterfaces.Any(i =>
            i.ToDisplayString() == "AttributedDI.IServiceModule");

        if (!implementsIServiceModule)
        {
            // Type has RegisterModule but doesn't implement IServiceModule - skip
            return null;
        }

        return new ModuleInfo(symbol);
    }

    /// <summary>
    /// Generates module registration code.
    /// </summary>
    /// <param name="sb">The string builder to append code to.</param>
    /// <param name="modules">The modules to generate code for.</param>
    public static void GenerateCode(StringBuilder sb, ImmutableArray<ModuleInfo> modules)
    {
        foreach (var module in modules)
        {
            string fullTypeName = module.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            _ = sb.AppendLine($"            new {fullTypeName}().ConfigureServices(services);");
        }
    }
}

internal sealed record ModuleInfo(INamedTypeSymbol TypeSymbol);
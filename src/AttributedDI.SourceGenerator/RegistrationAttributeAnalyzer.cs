using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator
{
    /// <summary>
    /// Analyzer for AttributedDI registration attributes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RegistrationAttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiagnosticDescriptors.NoInterfacesImplemented,
            DiagnosticDescriptors.IncompatibleServiceType,
            DiagnosticDescriptors.AbstractOrStaticType,
            DiagnosticDescriptors.DuplicateRegistration);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Get all registration attributes
            var registrationAttributes = namedTypeSymbol.GetAttributes()
                .Where(attr => IsRegistrationAttribute(attr.AttributeClass))
                .ToList();

            if (registrationAttributes.Count == 0)
                return;

            // ATDI003: Check if type is abstract or static
            if (namedTypeSymbol.IsAbstract || namedTypeSymbol.IsStatic)
            {
                foreach (var attr in registrationAttributes)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.AbstractOrStaticType,
                        attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                        namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                return;
            }

            // Check each attribute
            foreach (var attribute in registrationAttributes)
            {
                AnalyzeAttribute(context, namedTypeSymbol, attribute);
            }

            // ATDI004: Check for duplicate registrations
            CheckForDuplicates(context, namedTypeSymbol, registrationAttributes);
        }

        private static void AnalyzeAttribute(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, AttributeData attribute)
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass == null)
                return;

            var attributeName = attributeClass.Name;

            // ATDI001: RegisterAsImplementedInterfacesAttribute without interfaces
            if (attributeName == "RegisterAsImplementedInterfacesAttribute")
            {
                if (!namedTypeSymbol.AllInterfaces.Any())
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NoInterfacesImplemented,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                        namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            // ATDI002: RegisterAsAttribute with incompatible service type
            else if (attributeName == "RegisterAsAttribute")
            {
                if (attribute.ConstructorArguments.Length > 0)
                {
                    var serviceTypeArg = attribute.ConstructorArguments[0];
                    if (serviceTypeArg.Value is INamedTypeSymbol serviceType)
                    {
                        // Check if implementation type is assignable to service type
                        if (!IsAssignableTo(namedTypeSymbol, serviceType))
                        {
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.IncompatibleServiceType,
                                attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                                namedTypeSymbol.Name,
                                serviceType.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static void CheckForDuplicates(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, System.Collections.Generic.List<AttributeData> registrationAttributes)
        {
            var registrations = new System.Collections.Generic.HashSet<(string ServiceType, string Lifetime)>();

            foreach (var attribute in registrationAttributes)
            {
                var attributeClass = attribute.AttributeClass;
                if (attributeClass == null)
                    continue;

                var attributeName = attributeClass.Name;
                var lifetime = GetLifetime(attribute);

                if (attributeName == "RegisterAsSelfAttribute")
                {
                    var key = (namedTypeSymbol.ToDisplayString(), lifetime);
                    if (!registrations.Add(key))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateRegistration,
                            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                            namedTypeSymbol.Name,
                            namedTypeSymbol.Name,
                            lifetime);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else if (attributeName == "RegisterAsAttribute" && attribute.ConstructorArguments.Length > 0)
                {
                    var serviceTypeArg = attribute.ConstructorArguments[0];
                    if (serviceTypeArg.Value is INamedTypeSymbol serviceType)
                    {
                        var key = (serviceType.ToDisplayString(), lifetime);
                        if (!registrations.Add(key))
                        {
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateRegistration,
                                attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                                namedTypeSymbol.Name,
                                serviceType.Name,
                                lifetime);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
                else if (attributeName == "RegisterAsImplementedInterfacesAttribute")
                {
                    foreach (var iface in namedTypeSymbol.AllInterfaces)
                    {
                        var key = (iface.ToDisplayString(), lifetime);
                        if (!registrations.Add(key))
                        {
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.DuplicateRegistration,
                                attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                                namedTypeSymbol.Name,
                                iface.Name,
                                lifetime);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static string GetLifetime(AttributeData attribute)
        {
            var lifetimeArg = attribute.NamedArguments.FirstOrDefault(a => a.Key == "Lifetime").Value;
            if (lifetimeArg.Value != null)
            {
                return lifetimeArg.Value.ToString()!;
            }

            if (attribute.ConstructorArguments.Length > 0)
            {
                var lastArg = attribute.ConstructorArguments[attribute.ConstructorArguments.Length - 1];
                if (lastArg.Type?.Name == "ServiceLifetime")
                {
                    return lastArg.Value?.ToString() ?? "Transient";
                }
            }

            return "Transient";
        }

        private static bool IsRegistrationAttribute(INamedTypeSymbol? attributeClass)
        {
            if (attributeClass == null)
                return false;

            while (attributeClass != null)
            {
                if (attributeClass.ToDisplayString() == "AttributedDI.RegisterBase")
                    return true;

                attributeClass = attributeClass.BaseType;
            }

            return false;
        }

        private static bool IsAssignableTo(INamedTypeSymbol implementationType, INamedTypeSymbol serviceType)
        {
            // Check if types are the same
            if (SymbolEqualityComparer.Default.Equals(implementationType, serviceType))
                return true;

            // Check if implementation type inherits from service type
            var baseType = implementationType.BaseType;
            while (baseType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, serviceType))
                    return true;
                baseType = baseType.BaseType;
            }

            // Check if implementation type implements the service interface
            foreach (var iface in implementationType.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface, serviceType))
                    return true;
            }

            return false;
        }
    }
}


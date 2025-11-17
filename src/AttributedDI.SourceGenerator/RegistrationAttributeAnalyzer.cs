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
                // Handle generic RegisterAsAttribute<TService>
                if (attributeClass is { TypeArguments.Length: > 0 } namedAttr)
                {
                    var serviceType = namedAttr.TypeArguments[0] as INamedTypeSymbol;
                    if (serviceType != null)
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
            var lifetime = GetLifetime(context, namedTypeSymbol);

            foreach (var attribute in registrationAttributes)
            {
                var attributeClass = attribute.AttributeClass;
                if (attributeClass == null)
                    continue;

                var attributeName = attributeClass.Name;

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
                else if (attributeName == "RegisterAsAttribute")
                {
                    // Handle generic RegisterAsAttribute<TService>
                    if (attributeClass is { TypeArguments.Length: > 0 } namedAttr)
                    {
                        var serviceType = namedAttr.TypeArguments[0];
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

        private static string GetLifetime(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            // Check for lifetime attributes on the type
            var lifetimeAttributes = namedTypeSymbol.GetAttributes()
                .Where(attr => attr.AttributeClass != null && 
                              (attr.AttributeClass.ToDisplayString() == "AttributedDI.TransientAttribute" ||
                               attr.AttributeClass.ToDisplayString() == "AttributedDI.SingletonAttribute" ||
                               attr.AttributeClass.ToDisplayString() == "AttributedDI.ScopedAttribute"))
                .ToList();

            // Validate only one lifetime attribute
            if (lifetimeAttributes.Count > 1)
            {
                // Report diagnostic for multiple lifetime attributes
                foreach (var attr in lifetimeAttributes.Skip(1))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.DuplicateRegistration, // Reuse or create new descriptor
                        attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                        namedTypeSymbol.Name,
                        "multiple lifetime attributes",
                        "");
                    context.ReportDiagnostic(diagnostic);
                }
            }

            if (lifetimeAttributes.Count == 0)
                return "Transient"; // Default

            var lifetimeAttr = lifetimeAttributes[0];
            var lifetimeTypeName = lifetimeAttr.AttributeClass!.ToDisplayString();

            return lifetimeTypeName switch
            {
                "AttributedDI.TransientAttribute" => "Transient",
                "AttributedDI.SingletonAttribute" => "Singleton",
                "AttributedDI.ScopedAttribute" => "Scoped",
                _ => "Transient"
            };
        }

        private static bool IsRegistrationAttribute(INamedTypeSymbol? attributeClass)
        {
            if (attributeClass == null)
                return false;

            var fullName = attributeClass.ToDisplayString();
            
            return fullName == "AttributedDI.RegisterAsSelfAttribute" ||
                   fullName == "AttributedDI.RegisterAsImplementedInterfacesAttribute" ||
                   attributeClass.Name == "RegisterAsAttribute"; // Generic attribute
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


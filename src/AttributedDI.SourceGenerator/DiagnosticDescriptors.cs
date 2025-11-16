using Microsoft.CodeAnalysis;

namespace AttributedDI.SourceGenerator
{
    /// <summary>
    /// Diagnostic descriptors for AttributedDI analyzers.
    /// </summary>
    internal static class DiagnosticDescriptors
    {
        private const string Category = "AttributedDI";

        /// <summary>
        /// ATDI001: RegisterAsImplementedInterfacesAttribute used on type without interfaces.
        /// </summary>
        public static readonly DiagnosticDescriptor NoInterfacesImplemented = new(
            id: "ATDI001",
            title: "Type does not implement any interfaces",
            messageFormat: "Type '{0}' is marked with RegisterAsImplementedInterfacesAttribute but does not implement any interfaces",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Types marked with RegisterAsImplementedInterfacesAttribute must implement at least one interface.");

        /// <summary>
        /// ATDI002: RegisterAsAttribute with incompatible service type.
        /// </summary>
        public static readonly DiagnosticDescriptor IncompatibleServiceType = new(
            id: "ATDI002",
            title: "Service type is not assignable from implementation type",
            messageFormat: "Type '{0}' cannot be registered as '{1}' because it is not assignable from the implementation type",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The service type specified in RegisterAsAttribute must be assignable from the implementation type.");

        /// <summary>
        /// ATDI003: Abstract or static type with registration attribute.
        /// </summary>
        public static readonly DiagnosticDescriptor AbstractOrStaticType = new(
            id: "ATDI003",
            title: "Abstract or static type cannot be registered",
            messageFormat: "Type '{0}' is abstract or static and cannot be registered for dependency injection",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Only concrete, non-static types can be registered for dependency injection.");

        /// <summary>
        /// ATDI004: Duplicate service registration.
        /// </summary>
        public static readonly DiagnosticDescriptor DuplicateRegistration = new(
            id: "ATDI004",
            title: "Duplicate service registration",
            messageFormat: "Type '{0}' has duplicate registration for service type '{1}' with lifetime '{2}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The same service type and implementation type should not be registered multiple times with the same lifetime.");
    }
}


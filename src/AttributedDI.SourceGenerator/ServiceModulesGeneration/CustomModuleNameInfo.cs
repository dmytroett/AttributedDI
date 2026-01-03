namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

/// <summary>
/// Contains custom module, method, and namespace values from the GeneratedModuleAttribute.
/// </summary>
/// <param name="ModuleName">The custom module name from the attribute, or null to use default.</param>
/// <param name="MethodName">The custom method name from the attribute, or null to use default.</param>
/// <param name="Namespace">The custom namespace from the attribute, or null to derive from assembly name.</param>
internal sealed record CustomModuleNameInfo(string? ModuleName, string? MethodName, string? Namespace);

internal sealed record ResolvedModuleNames(string ModuleName, string MethodName, string Namespace);
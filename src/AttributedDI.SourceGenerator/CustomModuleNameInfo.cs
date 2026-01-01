namespace AttributedDI.SourceGenerator;

/// <summary>
/// Contains custom module and method names from the GeneratedModuleNameAttribute.
/// </summary>
/// <param name="ModuleName">The custom module name from the attribute, or null to use default.</param>
/// <param name="MethodName">The custom method name from the attribute, or null to use default.</param>
internal sealed record CustomModuleNameInfo(string? ModuleName, string? MethodName);

internal sealed record ResolvedModuleNames(string ModuleName, string MethodName);
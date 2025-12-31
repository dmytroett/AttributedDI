namespace AttributedDI.SourceGenerator;

/// <summary>
/// Contains assembly-level information for code generation.
/// </summary>
/// <param name="ModuleName">The custom module name from the attribute, or null to use default.</param>
/// <param name="MethodName">The custom method name from the attribute, or null to use default.</param>
/// <param name="AssemblyName">The name of the assembly being processed.</param>
internal sealed record AssemblyInfo(string? ModuleName, string? MethodName, string AssemblyName);
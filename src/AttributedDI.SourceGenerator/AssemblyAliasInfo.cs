namespace AttributedDI.SourceGenerator;

/// <summary>
/// Represents assembly information including name and optional method name alias.
/// </summary>
internal sealed record AssemblyInfo(
    string? MethodName,
    string AssemblyName);
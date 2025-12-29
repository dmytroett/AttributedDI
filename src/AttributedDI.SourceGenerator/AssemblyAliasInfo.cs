namespace AttributedDI.SourceGenerator;

/// <summary>
/// Represents assembly-level alias information for registration method naming.
/// </summary>
internal sealed record AssemblyAliasInfo(
    string MethodName,
    string AssemblyName);
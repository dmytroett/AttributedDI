using System;

namespace AttributedDI;

/// <summary>
/// Marks a generated service module to enable efficient discovery by source generators.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GeneratedModuleAttribute : Attribute
{
}
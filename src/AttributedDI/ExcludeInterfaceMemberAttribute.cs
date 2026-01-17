using System;

namespace AttributedDI;

/// <summary>
/// Excludes a member from generated interfaces.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeInterfaceMemberAttribute : Attribute
{
}
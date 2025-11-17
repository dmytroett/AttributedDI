using System;

namespace AttributedDI
{
    /// <summary>
    /// Marks the type to be registered with a singleton lifetime.
    /// When used alone, registers the type as itself.
    /// </summary>
    /// <remarks>
    /// Only one lifetime attribute can be applied to a type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class SingletonAttribute : Attribute
    {
    }
}


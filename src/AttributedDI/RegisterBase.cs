using System;
namespace AttributedDI
{
    /// <summary>
    /// Base class for all registration attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public abstract class RegisterBase : Attribute
    {
    }
}
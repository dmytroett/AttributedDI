using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI
{
    /// <summary>
    /// Extension methods for registering services marked with registration attributes.
    /// </summary>
    /// <remarks>
    /// The source generator creates registration methods for each assembly that contains marked types:
    /// <list type="bullet">
    /// <item><description><c>Add{AssemblyName}()</c> - Registers services from a specific assembly (or <c>Add{Alias}()</c> if <see cref="RegistrationAliasAttribute"/> is used)</description></item>
    /// </list>
    /// </remarks>
    public static partial class RegistrationServiceCollectionExtensions
    {
    }
}


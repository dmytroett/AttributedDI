using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AttributedDI;

/// <summary>
/// Represents configuration options for AttributedDI service registration.
/// </summary>
public sealed class AttributedDiOptions
{
    private readonly HashSet<IServiceModule> modules = new(ModuleTypeComparer.Instance);

    /// <summary>
    /// Gets or sets whether deferred service support is enabled.
    /// </summary>
    public bool EnableDeferred { get; set; } = true;

    /// <summary>
    /// Gets or sets whether owned service support is enabled.
    /// </summary>
    public bool EnableOwned { get; set; } = true;

    /// <summary>
    /// Gets or sets whether factory delegate support is enabled.
    /// </summary>
    public bool EnableFactory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether generated service modules should be registered.
    /// </summary>
    public bool RegisterGeneratedModules { get; set; } = true;

    /// <summary>
    /// Gets the additional modules to register.
    /// </summary>
    /// <remarks>Modules are deduplicated by concrete module type.</remarks>
    public IReadOnlyCollection<IServiceModule> AdditionalModules => modules.ToArray();

    /// <summary>
    /// Adds custom service module to register. 
    /// No need to add generated modules, they will be registered automatically if <see cref="RegisterGeneratedModules"/> is true.
    /// </summary>
    /// <param name="module">The service module to add.</param>
    /// <returns>The current options instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="module"/> is null.</exception>
    public AttributedDiOptions AddAdditionalModules(IServiceModule module)
    {
        modules.Add(module);

        return this;
    }

    private sealed class ModuleTypeComparer : IEqualityComparer<IServiceModule>
    {
        public static ModuleTypeComparer Instance { get; } = new();

        public bool Equals(IServiceModule? x, IServiceModule? y) => x?.GetType() == y?.GetType();

        public int GetHashCode([DisallowNull] IServiceModule obj) => obj.GetType().GetHashCode();
    }
}
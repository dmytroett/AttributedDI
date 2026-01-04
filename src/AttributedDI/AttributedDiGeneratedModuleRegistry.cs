using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AttributedDI;

/// <summary>
/// Provides a registry for generated service modules discovered at compile time.
/// </summary>
public static class AttributedDiGeneratedModuleRegistry
{
    private static readonly HashSet<RegisteredModuleType> Modules = new(RegisteredModuleTypeComparer.Instance);
    private static readonly object SyncRoot = new();

    /// <summary>
    /// Registers a generated service module type for later application.
    /// </summary>
    /// <param name="moduleType">The module type to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="moduleType"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="moduleType"/> does not implement <see cref="IServiceModule"/>.
    /// </exception>
    public static void Register(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        if (!typeof(IServiceModule).IsAssignableFrom(moduleType))
        {
            throw new ArgumentException("Module type must implement IServiceModule.", nameof(moduleType));
        }

        lock (SyncRoot)
        {
            _ = Modules.Add(new RegisteredModuleType(moduleType));
        }
    }

    internal static IReadOnlyCollection<RegisteredModuleType> GetRegisteredModules()
    {
        lock (SyncRoot)
        {
            return Modules.ToArray();
        }
    }

    internal readonly record struct RegisteredModuleType(
        [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type ModuleType);

    private sealed class RegisteredModuleTypeComparer : IEqualityComparer<RegisteredModuleType>
    {
        public static RegisteredModuleTypeComparer Instance { get; } = new();

        public bool Equals(RegisteredModuleType x, RegisteredModuleType y) => x.ModuleType == y.ModuleType;

        public int GetHashCode(RegisteredModuleType obj) => obj.ModuleType.GetHashCode();
    }
}
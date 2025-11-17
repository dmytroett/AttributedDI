using System;

namespace AttributedDI
{
    /// <summary>
    /// Assembly-level attribute that creates an alias for the registration method name.
    /// When applied, generates an Add{Alias}() extension method instead of Add{AssemblyName}().
    /// </summary>
    /// <example>
    /// <code>
    /// [assembly: RegistrationAlias("MyFeature")]
    /// </code>
    /// This will generate an AddMyFeature() method that registers all services from this assembly.
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class RegistrationAliasAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationAliasAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias name used to generate the registration method name.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="alias"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="alias"/> is empty or whitespace.</exception>
        public RegistrationAliasAttribute(string alias)
        {
            if (alias == null)
                throw new ArgumentNullException(nameof(alias));
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("Alias cannot be empty or whitespace.", nameof(alias));

            Alias = alias;
        }

        /// <summary>
        /// Gets the alias name used for the registration method.
        /// </summary>
        public string Alias { get; }
    }
}


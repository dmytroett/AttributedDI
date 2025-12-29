using AttributedDI;

[assembly: RegistrationAlias("LifetimeTests")]

namespace AssemblyWithLifetimeAttributes;

// Test class with only Transient attribute - should register as self
[Transient]
public class TransientOnlyType
{
}

// Test class with only Singleton attribute - should register as self
[Singleton]
public class SingletonOnlyType
{
}

// Test class with only Scoped attribute - should register as self
[Scoped]
public class ScopedOnlyType
{
}

// Test class with RegisterAsSelf and Singleton
[RegisterAsSelf]
[Singleton]
public class RegisterAsSelfWithSingleton
{
}

// Test class with RegisterAs and Scoped
public interface IMyService { }

[RegisterAs<IMyService>]
[Scoped]
public class RegisterAsWithScoped : IMyService
{
}

// Test class with RegisterAsImplementedInterfaces and Transient
public interface IInterface1 { }
public interface IInterface2 { }

[RegisterAsImplementedInterfaces]
[Transient]
public class RegisterAsInterfacesWithTransient : IInterface1, IInterface2
{
}

// Test class with no lifetime attribute (should default to Transient)
[RegisterAsSelf]
public class NoLifetimeAttribute
{
}
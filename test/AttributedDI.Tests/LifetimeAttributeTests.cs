using AssemblyWithLifetimeAttributes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace AttributedDI.Tests;

public class LifetimeAttributeTests
{
    [Fact]
    public void TransientAttribute_Alone_RegistersTypeAsSelfWithTransientLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(TransientOnlyType));
        descriptor.ImplementationType.Should().Be(typeof(TransientOnlyType));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void SingletonAttribute_Alone_RegistersTypeAsSelfWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(SingletonOnlyType));
        descriptor.ImplementationType.Should().Be(typeof(SingletonOnlyType));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ScopedAttribute_Alone_RegistersTypeAsSelfWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(ScopedOnlyType));
        descriptor.ImplementationType.Should().Be(typeof(ScopedOnlyType));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void RegisterAsSelf_WithSingleton_RegistersWithSingletonLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(RegisterAsSelfWithSingleton));
        descriptor.ImplementationType.Should().Be(typeof(RegisterAsSelfWithSingleton));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterAs_WithScoped_RegistersWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(IMyService));
        descriptor.ImplementationType.Should().Be(typeof(RegisterAsWithScoped));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void RegisterAsImplementedInterfaces_WithTransient_RegistersAllInterfacesWithTransientLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var interface1Descriptor = services.Single(x => x.ServiceType == typeof(IInterface1));
        interface1Descriptor.ImplementationType.Should().Be(typeof(RegisterAsInterfacesWithTransient));
        interface1Descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

        var interface2Descriptor = services.Single(x => x.ServiceType == typeof(IInterface2));
        interface2Descriptor.ImplementationType.Should().Be(typeof(RegisterAsInterfacesWithTransient));
        interface2Descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void NoLifetimeAttribute_DefaultsToTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLifetimeTests();

        // Assert
        var descriptor = services.Single(x => x.ServiceType == typeof(NoLifetimeAttribute));
        descriptor.ImplementationType.Should().Be(typeof(NoLifetimeAttribute));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegistrationAlias_GeneratesCorrectMethodName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Method should exist and compile
        services.AddLifetimeTests();
        services.Should().NotBeEmpty();
    }
}


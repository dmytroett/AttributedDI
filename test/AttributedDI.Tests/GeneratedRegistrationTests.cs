using AssemblyWithMultipleTypesToRegister;
using AssemblyWithSingleTypeWithMultipleRegisterAttributes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AttributedDI.Tests;

public class GeneratedRegistrationTests
{
    [Fact]
    public void Add_AllMarkedTypesInAssemblyAreRegistered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddAssemblyWithMultipleTypesToRegister();

        // assert
        services.Should().Contain(x => x.ServiceType == typeof(Type1) && x.ImplementationType == typeof(Type1));
        services.Should().Contain(x => x.ServiceType == typeof(Type2) && x.ImplementationType == typeof(Type2));
    }

    [Fact]
    public void Add_RegisterAsImplementedInterfaces_RegistersAllInterfaces()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddAssemblyWithSingleTypeWithMultipleRegisterAttributes();

        // assert
        services.Should().Contain(x => x.ServiceType == typeof(Interface1) && x.ImplementationType == typeof(TypeWithMultipleRegisterAttributes));
        services.Should().Contain(x => x.ServiceType == typeof(Interface2) && x.ImplementationType == typeof(TypeWithMultipleRegisterAttributes));
    }
}
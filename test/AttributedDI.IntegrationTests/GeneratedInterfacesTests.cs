using GeneratedInterfacesSut;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel;

namespace AttributedDI.IntegrationTests;

public class GeneratedInterfacesTests
{
    [Fact]
    public void ServicesRegisterCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGeneratedInterfacesSut();

        // assert
        AssertContainsService<IMyTransientClassToGenerateInterface, MyTransientClassToGenerateInterface>(services, ServiceLifetime.Transient);
        AssertContainsService<IMyScopedClassToGenerateInterface, MyScopedClassToGenerateInterface>(services, ServiceLifetime.Scoped);
        AssertContainsService<IShouldGenerateEmptyInterface, ShouldGenerateEmptyInterface>(services, ServiceLifetime.Transient);
        AssertDoesNotContainService<GeneratesInterfaceButDoesntRegister>(services);

        Assert.True(typeof(IGeneratesInterfaceButDoesntRegister) != null);
    }

    [Fact]
    public void WellKnownInterfacesAndMembersHandledCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGeneratedInterfacesSut();

        // assert
        AssertContainsService<IClassWithABunchOfKnownInterfaces, ClassWithABunchOfKnownInterfaces>(services, ServiceLifetime.Transient);

        var @interfaceType = typeof(IClassWithABunchOfKnownInterfaces);

        var members = @interfaceType.GetMembers();

        foreach (var member in members)
        {
            Assert.NotEqual(nameof(INotifyPropertyChanged.PropertyChanged), member.Name);
            Assert.NotEqual(nameof(IComparable.CompareTo), member.Name);
            Assert.NotEqual(nameof(IEquatable<object>.Equals), member.Name);
            Assert.NotEqual(nameof(IDisposable.Dispose), member.Name);
            Assert.NotEqual(nameof(IEnumerable.GetEnumerator), member.Name);
        }
    }

    [Fact]
    public void RegisterAsgeneratedInterfaceCorrectlyHandlesCustomNamespaces()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGeneratedInterfacesSut();

        AssertContainsService<GeneratedInterfacesSut.Abstractions.ICustomNamespaceViaParameter, CustomNamespaceViaParameter>(services, ServiceLifetime.Transient);
        AssertContainsService<GeneratedInterfacesSut.Contracts.ICustomInterface1, CustomNamespaceViaFullyQualifiedName>(services, ServiceLifetime.Transient);
        AssertContainsService<GeneratedInterfacesSut.Internal.ICustomInterface2, CustomNamespaceViaBoth>(services, ServiceLifetime.Transient);
    }

    [Fact]
    public void GenerateInterfaceCorrectlyHandlesCustomNamespaces()
    {
        Assert.NotNull(typeof(GeneratedInterfacesSut.Abstractions.ICustomNamespaceViaParameterG));
        Assert.NotNull(typeof(GeneratedInterfacesSut.Contracts.ICustomInterface1G));
        Assert.NotNull(typeof(GeneratedInterfacesSut.Internal.ICustomInterface2G));
    }
}
using GeneratedInterfacesSut;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

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

        Assert.NotNull(typeof(IGeneratesInterfaceButDoesntRegister));
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

        var members = @interfaceType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var memberNames = members.Select(member => member.Name).ToArray();

        Assert.DoesNotContain(nameof(INotifyPropertyChanged.PropertyChanged), memberNames);
        Assert.DoesNotContain(nameof(IComparable.CompareTo), memberNames);
        Assert.DoesNotContain(nameof(IEquatable<object>.Equals), memberNames);
        Assert.DoesNotContain(nameof(IDisposable.Dispose), memberNames);
        Assert.DoesNotContain(nameof(IEnumerable.GetEnumerator), memberNames);

        Assert.Contains(nameof(IClassWithABunchOfKnownInterfaces.DoWork), memberNames);
    }

    [Fact]
    public void RegisterAsGeneratedInterfaceCorrectlyHandlesCustomNamespaces()
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

    [Fact]
    public void ExcludedMembersHandledCorrectly()
    {
        var type = typeof(IWithExcludedMembers);

        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var memberNames = members.Select(member => member.Name).ToArray();

        Assert.Contains(nameof(WithExcludedMembers.IncludedMethod), memberNames);
        Assert.Contains(nameof(WithExcludedMembers.IncludedProperty), memberNames);
        Assert.Contains(nameof(WithExcludedMembers.IncludedEvent), memberNames);

        Assert.DoesNotContain(nameof(WithExcludedMembers.ExcludedMethod), memberNames);
        Assert.DoesNotContain(nameof(WithExcludedMembers.ExcludedProperty), memberNames);
        Assert.DoesNotContain(nameof(WithExcludedMembers.ExcludedEvent), memberNames);

        Assert.NotNull(type.GetMethod(nameof(WithExcludedMembers.IncludedMethod), Type.EmptyTypes));
        Assert.Null(type.GetMethod(nameof(WithExcludedMembers.ExcludedMethod), Type.EmptyTypes));
        Assert.NotNull(type.GetProperty(nameof(WithExcludedMembers.IncludedProperty)));
        Assert.Null(type.GetProperty(nameof(WithExcludedMembers.ExcludedProperty)));
        Assert.NotNull(type.GetEvent(nameof(WithExcludedMembers.IncludedEvent)));
        Assert.Null(type.GetEvent(nameof(WithExcludedMembers.ExcludedEvent)));

        var indexers = members
            .Where(x => x.MemberType == MemberTypes.Property)
            .Cast<PropertyInfo>()
            .Where(x => x.GetIndexParameters().Length > 0)
            .ToArray();

        Assert.Contains(indexers, indexer => indexer.GetIndexParameters().Single().ParameterType == typeof(int));
        Assert.DoesNotContain(indexers, indexer => indexer.GetIndexParameters().Single().ParameterType == typeof(string));
        Assert.Single(indexers);
    }
}
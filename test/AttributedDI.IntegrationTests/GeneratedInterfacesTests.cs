using GeneratedInterfacesSut;
using Microsoft.Extensions.DependencyInjection;

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
        AssertContainsService<IShouldNotGenerateInterfaceWithDisposable, ShouldNotGenerateInterfaceWithDisposable>(services, ServiceLifetime.Transient);
        AssertContainsService<IShouldGenerateInterfaceWithDisposableAndOtherMembers, ShouldGenerateInterfaceWithDisposableAndOtherMembers>(services, ServiceLifetime.Transient);
        AssertDoesNotContainService<GeneratesInterfaceButDoesntRegister>(services);

        Assert.True(typeof(IGeneratesInterfaceButDoesntRegister) != null);
    }
}
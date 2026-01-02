using Microsoft.Extensions.DependencyInjection;
using GeneratedInterfacesSut;

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
        AssertDoesNotContainService<ShouldNotGenerateInterfaceWithDisposable>(services);
        AssertContainsService<IShouldGenerateInterfaceWithDisposableAndOtherMembers, ShouldGenerateInterfaceWithDisposableAndOtherMembers>(services, ServiceLifetime.Transient);
        AssertDoesNotContainService<GeneratesInterfaceButDoesntRegister>(services);

        Assert.True(typeof(IGeneratesInterfaceButDoesntRegister) != null);
    }
}
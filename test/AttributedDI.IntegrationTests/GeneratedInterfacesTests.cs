using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests;

public class GeneratedInterfacesTests
{
    [Fact]
    public void GeneratedInterfaceRegistration_WorksAsExpected()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAttributedDI(typeof(Company.TeamName.Project.API.MyClassToGenerateInterface).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var myClassInstance = serviceProvider.GetService<Company.TeamName.Project.API.IMyClassToGenerateInterface>();

        // Assert
        Assert.NotNull(myClassInstance);
        Assert.IsType<Company.TeamName.Project.API.MyClassToGenerateInterface>(myClassInstance);
    }
}
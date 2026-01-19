using GenerateExtensions.OptOut;

namespace AttributedDI.GenerateExtensionsOptOut.IntegrationTests;

public class GenerateExtensionsOptOutTests
{
    [Fact]
    public void DoesNotGenerateAddAttributedDi()
    {
        var generatedType = typeof(OptOutService).Assembly
            .GetType("AttributedDI.AttributedDiGeneratedServiceCollectionExtensions");

        Assert.Null(generatedType);
    }
}
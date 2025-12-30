namespace AttributedDI.SourceGenerator.UnitTests;

public class BasicServicesRegistrationTests
{
    [Fact]
    public async Task RegistersAsSelfCorrectly()
    {
        var code = """
                   using AttributedDI;

                   namespace MyNamespace
                   {
                       [RegisterAsSelf]
                       public class MyService
                       {
                       }
                   }
                   """;
        
        await TestHelper.CompileAndVerify(code);
    }
}
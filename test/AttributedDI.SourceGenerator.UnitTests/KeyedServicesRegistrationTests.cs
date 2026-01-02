namespace AttributedDI.SourceGenerator.UnitTests;

public class KeyedServicesRegistrationTests
{
    [Fact]
    public async Task RegistersKeyedServiceAsSelf()
    {
        // Tests: RegisterAsSelf with key
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsSelf("primary")]
                       public class MyService { }

                       [RegisterAsSelf("secondary"), Singleton]
                       public class OtherService { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersKeyedServiceAsImplementedInterfaces()
    {
        // Tests: RegisterAsImplementedInterfaces with key
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IService { }
                       public interface IRepository { }

                       [RegisterAsImplementedInterfaces("primary")]
                       public class ServiceImpl : IService { }

                       [RegisterAsImplementedInterfaces("multi"), Singleton]
                       public class MultiImpl : IService, IRepository { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersSameServiceWithDifferentKeys()
    {
        // Tests: Multiple registrations of the same type with different keys
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IMyService { }

                       [RegisterAs<IMyService>("primary")]
                       [RegisterAs<IMyService>("secondary")]
                       [Transient]
                       public class MyServiceImpl : IMyService { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersKeyedServiceWithEnumKeys()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public enum ServiceKey
                       {
                           Primary,
                           Secondary
                       }

                       public interface IMyService { }

                       [RegisterAs<IMyService>(ServiceKey.Primary)]
                       public class EnumKeyedService : IMyService { }

                       [RegisterAsSelf(ServiceKey.Secondary)]
                       public class EnumKeyedSelf { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersKeyedTypeAgainstGeneratedInterface()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsGeneratedInterface("IGenerated", key: "primary")]
                       public partial class GeneratedService
                       {
                           public string Ping() => "pong";
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersKeyedAndNonKeyedServicesTogether()
    {
        // Tests: Mix of keyed and non-keyed registrations
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IMyService { }

                       // Non-keyed registration
                       [RegisterAs<IMyService>]
                       public class DefaultServiceImpl : IMyService { }

                       // Keyed registrations
                       [RegisterAs<IMyService>("v1")]
                       [RegisterAs<IMyService>("v2")]
                       [Singleton]
                       public class VersionedServiceImpl : IMyService { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task RegistersKeyedServiceWithIntegerKey()
    {
        // Tests: Using integer as key
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IMyService { }

                       [RegisterAs<IMyService>(1)]
                       public class Service1 : IMyService { }

                       [RegisterAs<IMyService>(2), Scoped]
                       public class Service2 : IMyService { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }
}
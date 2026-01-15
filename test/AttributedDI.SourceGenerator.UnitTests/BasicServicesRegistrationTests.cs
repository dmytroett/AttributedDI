namespace AttributedDI.SourceGenerator.UnitTests;

public class BasicServicesRegistrationTests
{
    [Fact]
    public async Task RegistersServicesWithDifferentLifetimesAndInterfaces()
    {
        // Tests: RegisterAsSelf with lifetimes plus interface registrations
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IMyService { }
                       public interface IOtherService { }

                       [Transient]
                       public class ShouldBeRegisteredAsTransient { } 
                   
                       [RegisterAsSelf]
                       public class TransientService { }

                       [RegisterAsSelf, Singleton]
                       public class SingletonService { }

                       [RegisterAsSelf, Scoped]
                       public class ScopedService { }

                       [RegisterAs<IMyService>]
                       public class MyServiceImpl : IMyService { }

                       [RegisterAs<IOtherService>, Singleton]
                       public class OtherServiceImpl : IOtherService { }
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
    public async Task RegistersAsImplementedInterfaces()
    {
        // Tests: RegisterAsImplementedInterfaces for types implementing multiple interfaces
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IService { }
                       public interface IRepository { }

                       [RegisterAsImplementedInterfaces]
                       public class ServiceImpl : IService { }

                       [RegisterAsImplementedInterfaces, Scoped]
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
    public async Task RegistersMultipleServicesFromSameType()
    {
        // Tests: Multiple registration attributes on single type
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IService { }
                       public interface IRepository { }

                       [RegisterAsSelf]
                       [RegisterAs<IService>]
                       [RegisterAs<IRepository>]
                       [Singleton]
                       public class MultiRegistration : IService, IRepository { }
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
    public async Task UsesUserSuppliedNamesWhenProvided()
    {
        var code = """
                   using AttributedDI;

                   [assembly: GeneratedModuleName(moduleName: "MyModule", methodName: "AddTheModule", moduleNamespace: "Custom.Namespace")]

                   namespace MyApp
                   {
                       [RegisterAsSelf]
                       public class MyService { }
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
    public async Task HandlesEmptyAssembly()
    {
        // Tests: No services to register (should generate nothing)
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public class RegularClass { }
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
    public async Task HandlesDisposable()
    {
        // Tests: Service implementing IDisposable and IAsyncDisposable
        var code = """
                   using AttributedDI;
                   using System;
                   using System.Threading.Tasks;

                   namespace MyApp
                   {
                       public interface IService { }

                       [RegisterAsImplementedInterfaces]
                       public class DisposableService : IService, IDisposable, IAsyncDisposable
                       {
                           public void Dispose() { }

                           public ValueTask DisposeAsync() => ValueTask.CompletedTask;
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
    public async Task RegistersServicesInNestedNamespaces()
    {
        // Tests: Services across different nested namespaces
        var code = """
                   using AttributedDI;

                   namespace MyApp.Services
                   {
                       [RegisterAsSelf, Singleton]
                       public class OuterService { }
                   }

                   namespace MyApp.Services.Internal
                   {
                       [RegisterAsSelf]
                       public class InnerService { }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact(Skip = "Pending diagnostics for conflicting lifetime attributes on a single type.")]
    public async Task ConflictingLifetimeAttributesEmitDiagnostics()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsSelf]
                       [Singleton]
                       [Scoped]
                       public class ConflictingLifetimes { }
                   }
                   """;

        var (_, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.NotEmpty(diagnostics);
    }
}
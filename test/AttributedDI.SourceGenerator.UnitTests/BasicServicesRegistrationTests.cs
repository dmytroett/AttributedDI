namespace AttributedDI.SourceGenerator.UnitTests;

public class BasicServicesRegistrationTests
{
    [Fact]
    public async Task RegistersServicesAcrossCommonScenarios()
    {
        // Tests: RegisterAsSelf/implemented interfaces, multiple registrations, disposables, and nested namespaces.
        var code = """
                   using AttributedDI;
                   using System;
                   using System.Threading.Tasks;

                   namespace MyApp
                   {
                       public interface IMyService { }
                       public interface IOtherService { }
                       public interface IService { }
                       public interface IRepository { }

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

                       [RegisterAsImplementedInterfaces]
                       public class ServiceImpl : IService { }

                       [RegisterAsImplementedInterfaces, Scoped]
                       public class MultiImpl : IService, IRepository { }

                       [RegisterAsSelf]
                       [RegisterAs<IService>]
                       [RegisterAs<IRepository>]
                       [Singleton]
                       public class MultiRegistration : IService, IRepository { }

                       [RegisterAsImplementedInterfaces]
                       public class DisposableService : IService, IDisposable, IAsyncDisposable
                       {
                           public void Dispose() { }

                           public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                       }
                   }

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

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

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

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

        await Verify(output);
    }

    [Fact]
    public void HandlesEmptyAssembly()
    {
        // Tests: No services to register (should generate nothing)
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public class RegularClass { }
                   }
                   """;

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

        Assert.DoesNotContain("RegularClass", output);
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

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRunGenerators();

        Assert.NotEmpty(result.SourceGeneratorDiagnostics);
    }
}
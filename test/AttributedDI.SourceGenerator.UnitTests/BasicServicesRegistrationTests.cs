namespace AttributedDI.SourceGenerator.UnitTests;

public class BasicServicesRegistrationTests
{
    [Fact]
    public async Task RegistersServicesWithDifferentLifetimes()
    {
        // Tests: RegisterAsSelf with Transient (default), Singleton, and Scoped
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [Transient]
                       public class ShouldBeRegisteredAsTransient { } 
                   
                       [RegisterAsSelf]
                       public class TransientService { }

                       [RegisterAsSelf, Singleton]
                       public class SingletonService { }

                       [RegisterAsSelf, Scoped]
                       public class ScopedService { }
                   }
                   """;
        
        await TestHelper.CompileAndVerify(code);
    }

    [Fact]
    public async Task RegistersAsSpecificInterface()
    {
        // Tests: RegisterAs<T> with different lifetimes
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IMyService { }
                       public interface IOtherService { }

                       [RegisterAs<IMyService>]
                       public class MyServiceImpl : IMyService { }

                       [RegisterAs<IOtherService>, Singleton]
                       public class OtherServiceImpl : IOtherService { }
                   }
                   """;
        
        await TestHelper.CompileAndVerify(code);
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
        
        await TestHelper.CompileAndVerify(code);
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
        
        await TestHelper.CompileAndVerify(code);
    }

    [Fact]
    public async Task RegistersServiceModule()
    {
        // Tests: IServiceModule with RegisterModule attribute
        var code = """
                   using AttributedDI;
                   using Microsoft.Extensions.DependencyInjection;

                   namespace MyApp
                   {
                       public interface ILogger { }
                       public interface IRepository { }

                       public class Logger : ILogger { }

                       public class UserRepository : IRepository { }

                       [RegisterModule]
                       public class MyModule : IServiceModule
                       {
                           public void ConfigureServices(IServiceCollection services)
                           {
                               services.AddSingleton<ILogger, Logger>();
                               services.AddTransient<IRepository, UserRepository>();
                           }
                       }
                   }
                   """;
        
        await TestHelper.CompileAndVerify(code);
    }

    [Fact]
    public async Task GeneratesCustomMethodName()
    {
        var code = """
                   using AttributedDI;

                   [assembly: RegistrationMethodName("MyCustomServices")]

                   namespace MyApp
                   {
                       [RegisterAsSelf]
                       public class MyService { }
                   }
                   """;
        
        await TestHelper.CompileAndVerify(code);
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
        
        await TestHelper.CompileAndVerify(code);
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
        
        await TestHelper.CompileAndVerify(code);
    }
}

namespace AttributedDI.SourceGenerator.UnitTests;

public class InterfaceGenerationTests
{
    [Fact]
    public async Task GeneratesInterfaceWithDefaultNamingAndSkipsDisposableMembers()
    {
        var code = """
                   using AttributedDI;
                   using System;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public class Foo : IDisposable
                       {
                           public void DoWork() { }

                           public void Dispose() { }
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
    public async Task GeneratesInterfaceWithCustomNameAndNamespace()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface("ICustom", "Custom.Contracts")]
                       public class Bar
                       {
                           public int GetValue() => 42;
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
    public async Task RegistersTypeAgainstGeneratedInterface()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsGeneratedInterface("ICustomService", "Contracts")]
                       public class GeneratedService
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
    public async Task GeneratesInterfaceForGenericClassWithSingleTypeParameter()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public class Repository<T>
                       {
                           public T GetById(int id) => default!;

                           public void Save(T item) { }
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
    public async Task GeneratesInterfaceForGenericClassWithMultipleTypeParameters()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public class Mapper<TSource, TDestination>
                       {
                           public TDestination Map(TSource source) => default!;

                           public TSource ReverseMap(TDestination destination) => default!;
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
    public async Task GeneratesInterfaceForGenericClassWithConstraints()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public class Validator<T> where T : class
                       {
                           public bool Validate(T item) => true;

                           public T CreateDefault() => null!;
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
    public async Task RegistersGenericTypeAgainstGeneratedInterface()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsGeneratedInterface("IRepository<TEntity>")]
                       public class GenericRepository<TEntity>
                       {
                           public TEntity GetById(int id) => default!;

                           public void Add(TEntity entity) { }

                           public void Remove(TEntity entity) { }
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
}
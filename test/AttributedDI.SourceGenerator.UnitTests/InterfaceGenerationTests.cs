using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Globalization;

namespace AttributedDI.SourceGenerator.UnitTests;

public class InterfaceGenerationTests
{
    private static void AssertGeneratedCodeCompiles(CSharpCompilation originalCompilation, GeneratorDriver driver)
    {
        // Get the generated syntax trees from the driver's run result
        var runResult = driver.GetRunResult();
        var generatedSyntaxTrees = runResult.GeneratedTrees;

        // Create a new compilation with both original and generated sources
        var compilationWithGenerated = originalCompilation.AddSyntaxTrees(generatedSyntaxTrees);

        // Check for compilation errors
        var errors = compilationWithGenerated
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (errors.Count != 0)
        {
            var errorMessages = string.Join(
                Environment.NewLine,
                errors.Select(static e => $"{e.Id}: {e.GetMessage(CultureInfo.InvariantCulture)}"));
            throw new AssertionException($"Generated code does not compile:\n{errorMessages}");
        }
    }

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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        // Verify snapshot output
        await Verify(driver);

        // Verify the generated code compiles with the original source
        AssertGeneratedCodeCompiles(compilation, driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
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

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
    }
}
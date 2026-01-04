using System.ComponentModel;

namespace AttributedDI.SourceGenerator.UnitTests;

public class InterfaceGenerationTests
{
    [Fact]
    public async Task GeneratesInterfaceWithDefaultNamingAndSkipsKnownInterfacesMembers()
    {
        var code = """
                   using AttributedDI;
                   using System;
                   using System.Collections;
                   using System.Collections.Generic;
                   using System.ComponentModel;
                   using System.Threading.Tasks;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class GenericListHost : IList<int>, IReadOnlyList<int>, INotifyPropertyChanged, IDisposable, IAsyncDisposable, IComparable, IComparable<GenericListHost>, IEquatable<GenericListHost>
                       {
                           public event PropertyChangedEventHandler? PropertyChanged;

                           public void Custom() { }

                           public void Dispose() { }

                           public ValueTask DisposeAsync() => ValueTask.CompletedTask;

                           public int CompareTo(object? obj) => throw new NotImplementedException();

                           public int CompareTo(GenericListHost? other) => throw new NotImplementedException();

                           public bool Equals(GenericListHost? other) => throw new NotImplementedException();

                           public int this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                           public int Count => throw new NotImplementedException();

                           public bool IsReadOnly => throw new NotImplementedException();

                           public void Add(int item) => throw new NotImplementedException();

                           public void Clear() => throw new NotImplementedException();

                           public bool Contains(int item) => throw new NotImplementedException();

                           public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();

                           public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();

                           public int IndexOf(int item) => throw new NotImplementedException();

                           public void Insert(int index, int item) => throw new NotImplementedException();

                           public bool Remove(int item) => throw new NotImplementedException();

                           public void RemoveAt(int index) => throw new NotImplementedException();

                           IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
                       }

                       [GenerateInterface]
                       public partial class NonGenericListHost : IList
                       {
                           public void CustomNonGeneric() { }

                           public int Add(object? value) => throw new NotImplementedException();

                           public void Clear() => throw new NotImplementedException();

                           public bool Contains(object? value) => throw new NotImplementedException();

                           public int IndexOf(object? value) => throw new NotImplementedException();

                           public void Insert(int index, object? value) => throw new NotImplementedException();

                           public bool IsFixedSize => throw new NotImplementedException();

                           public bool IsReadOnly => throw new NotImplementedException();

                           public void Remove(object? value) => throw new NotImplementedException();

                           public void RemoveAt(int index) => throw new NotImplementedException();

                           public object? this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                           public int Count => throw new NotImplementedException();

                           public bool IsSynchronized => throw new NotImplementedException();

                           public object SyncRoot => throw new NotImplementedException();

                           public void CopyTo(Array array, int index) => throw new NotImplementedException();

                           public IEnumerator GetEnumerator() => throw new NotImplementedException();
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithExtraReferences(typeof(INotifyPropertyChanged).Assembly)
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
                       public partial class Bar
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
    public async Task GeneratesInterfaceForGenericClassWithSingleTypeParameter()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class Repository<T>
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
                       public partial class Mapper<TSource, TDestination>
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
                       public partial class Validator<T> where T : class
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
                       [RegisterAsGeneratedInterface("IRepository<>")]
                       public partial class GenericRepository<TEntity>
                       {
                           public TEntity GetById(int id) => default!;

                           public void Add(TEntity entity) { }

                           public void Remove(TEntity entity) { }
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithExtraReferences(typeof(IServiceProvider).Assembly)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact(DisplayName = "Generates and registers generic interface with custom name and two generic parameters with type constraints")]
    public async Task GeneratesAndRegistersGenericInterfaceWithCustomNameAndTwoGenericParametersWithConstraints()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsGeneratedInterface("ITransformation<,>")]
                       public partial class GenericTransformation<TInput, TOutput>
                           where TOutput : class, new()
                       {
                           public TOutput Transform(TInput source) => default!;
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithExtraReferences(typeof(IServiceProvider).Assembly)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task GeneratesInterfaceWithPropertiesIndexersAndAsyncMembers()
    {
        var code = """
                   using AttributedDI;
                   using System;
                   using System.Threading.Tasks;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class ComplexType
                       {
                           public string Name { get; } = "name";

                           public event EventHandler? Changed;

                           public int this[int index] { get => 0; set { } }

                           public Task<int> GetValueAsync(int index) => Task.FromResult(index);
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
    public async Task GeneratesInterfaceExcludingNonPublicMembers()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class VisibilitySample
                       {
                           public void Allowed() { }

                           internal void InternalOnly() { }

                           protected void ProtectedOnly() { }

                           private void PrivateOnly() { }
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
    public async Task SkipsExplicitInterfaceImplementations()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public interface IExplicitContract
                       {
                           void Hidden();

                           int Value { get; }
                       }

                       [GenerateInterface]
                       public partial class ExplicitImplementationSample : IExplicitContract
                       {
                           void IExplicitContract.Hidden() { }

                           int IExplicitContract.Value => 42;

                           public void Visible() { }
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
    public async Task GeneratesInterfaceFromPartialDeclarations()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class PartialExample
                       {
                           public void FromFirst() { }
                       }

                       public partial class PartialExample
                       {
                           public void FromSecond() { }
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
    public async Task GeneratesInterfaceWithInheritedAndOverriddenMembers()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public class Base
                       {
                           public void BaseOnly() { }

                           public virtual void Execute() { }
                       }

                       [GenerateInterface]
                       public partial class Derived : Base
                       {
                           public override void Execute() { }

                           public void DerivedOnly() { }
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
    public async Task GeneratesInterfaceWithComplexGenericConstraints()
    {
        var code = """
                   using AttributedDI;
                   using System;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class Processor<TInput, TOutput>
                           where TInput : class, IDisposable, new()
                           where TOutput : struct
                       {
                           public TOutput Process(TInput input) => default!;

                           public TOutput CreateDefault() => default!;
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
    public async Task SkipsNestedTypes()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public partial class Outer
                       {
                           [GenerateInterface]
                           public partial class Inner
                           {
                               public void DoWork() { }
                           }
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);
        Assert.Empty(output);
        Assert.True(string.IsNullOrWhiteSpace(output));
        await Verify(output);
    }

    [Fact]
    public async Task SkipsRefReturnsAndRefLikeParameters()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public partial class RefMembers
                       {
                           public ref int RefReturn(ref int value) => ref value;

                           public int InParameter(in int value) => value;

                           public int OutParameter(out int value)
                           {
                               value = 1;
                               return value;
                           }

                           public void Allowed() { }

                           private ref int PrivateRefReturn(ref int value) => ref value;
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);
        Assert.Contains("void Allowed()", output);
        Assert.DoesNotContain("RefReturn", output);
        Assert.DoesNotContain("InParameter", output);
        Assert.DoesNotContain("OutParameter", output);
        Assert.DoesNotContain("PrivateRefReturn", output);
        await Verify(output);
    }

    [Fact(Skip = "Pending diagnostics for invalid GenerateInterface usage on non-class targets.")]
    public async Task GenerateInterfaceOnInterfaceEmitsDiagnostic()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public interface IFoo
                       {
                           void DoWork();
                       }
                   }
                   """;

        var (_, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.NotEmpty(diagnostics);
    }
}
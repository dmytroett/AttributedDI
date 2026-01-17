using AttributedDI;
using System.Collections;
using System.ComponentModel;

namespace GeneratedInterfacesSut;

[RegisterAsGeneratedInterface]
public partial class MyTransientClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface, Scoped]
public partial class MyScopedClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface]
public partial class ShouldGenerateEmptyInterface : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        // Dispose resources
    }

    public ValueTask DisposeAsync()
    {
        // Async dispose resources
        return ValueTask.CompletedTask;
    }
}

[GenerateInterface]
public partial class GeneratesInterfaceButDoesntRegister
{
    public void PerformAction()
    {
        Console.WriteLine("Performing action...");
    }
}

[RegisterAsGeneratedInterface]
public partial class ClassWithABunchOfKnownInterfaces :
    INotifyPropertyChanged, IDisposable, IEquatable<ClassWithABunchOfKnownInterfaces>, IComparable<ClassWithABunchOfKnownInterfaces>, IEnumerable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void DoWork()
    {
        throw new NotImplementedException();
    }

    public int CompareTo(ClassWithABunchOfKnownInterfaces? other)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool Equals(ClassWithABunchOfKnownInterfaces? other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        return !(left == right);
    }

    public static bool operator <(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(ClassWithABunchOfKnownInterfaces left, ClassWithABunchOfKnownInterfaces right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}

[RegisterAsGeneratedInterface(interfaceNamespace: "GeneratedInterfacesSut.Abstractions")]
public partial class CustomNamespaceViaParameter { }

[RegisterAsGeneratedInterface("GeneratedInterfacesSut.Contracts.ICustomInterface1")]
public partial class CustomNamespaceViaFullyQualifiedName { }

[RegisterAsGeneratedInterface("ICustomInterface2", "GeneratedInterfacesSut.Internal")]
public partial class CustomNamespaceViaBoth { }

[GenerateInterface(interfaceNamespace: "GeneratedInterfacesSut.Abstractions")]
public partial class CustomNamespaceViaParameterG { }

[GenerateInterface("GeneratedInterfacesSut.Contracts.ICustomInterface1G")]
public partial class CustomNamespaceViaFullyQualifiedNameG { }

[GenerateInterface("ICustomInterface2G", "GeneratedInterfacesSut.Internal")]
public partial class CustomNamespaceViaBothG { }

[GenerateInterface]
public partial class WithExcludedMembers
{
    public void IncludedMethod() { }

    [ExcludeInterfaceMember]
    public void ExcludedMethod() { }

    public int IncludedProperty { get; set; }

    [ExcludeInterfaceMember]
    public int ExcludedProperty { get; set; }

    public event EventHandler<EventArgs>? IncludedEvent;

    [ExcludeInterfaceMember]
    public event EventHandler<EventArgs>? ExcludedEvent;

    public int this[int i] { get => 0; }

    [ExcludeInterfaceMember]
    public string this[string str] { get => string.Empty; }
}
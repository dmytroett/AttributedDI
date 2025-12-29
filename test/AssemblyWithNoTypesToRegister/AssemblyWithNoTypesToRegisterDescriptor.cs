using System.Reflection;

namespace AssemblyWithNoTypesToRegister;

public class AssemblyWithNoTypesToRegisterDescriptor
{
    public Assembly Assembly { get; } = typeof(AssemblyWithNoTypesToRegisterDescriptor).Assembly;
}
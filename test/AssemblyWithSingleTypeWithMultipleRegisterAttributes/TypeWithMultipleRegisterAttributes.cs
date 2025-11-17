using AttributedDI;

namespace AssemblyWithSingleTypeWithMultipleRegisterAttributes
{
    [RegisterAs<Interface1>]
    [RegisterAs<Interface2>]
    public class TypeWithMultipleRegisterAttributes : Interface1, Interface2
    {
    }
}
using AttributedDI;

[assembly: RegistrationMethodName("AddMyAmazingCustomServices")]

namespace CustomRegistrationMethodName;

[RegisterAsSelf]
[Scoped]
public class AliasedAssemblyService
{
}


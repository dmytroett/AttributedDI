using AttributedDI;

[assembly: GeneratedModule(methodName: "AddMyAmazingCustomServices", moduleName: "MyIncredibleCustomModule", moduleNamespace:"MyUnbelievableNamespace")]

namespace CustomRegistrationMethodName;

[RegisterAsSelf]
[Scoped]
public class AliasedAssemblyService
{
}
using AttributedDI;

[assembly: GeneratedModule(methodName: "AddMyAmazingCustomServices", moduleName: "MyIncredibleCustomModule")]

namespace CustomRegistrationMethodName;

[RegisterAsSelf]
[Scoped]
public class AliasedAssemblyService
{
}
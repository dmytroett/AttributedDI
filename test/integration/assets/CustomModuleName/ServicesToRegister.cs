using AttributedDI;

[assembly: ServiceCollectionExtension(methodName: "AddMyAmazingCustomServices", extensionClassName: "MyIncredibleCustomModule", extensionNamespace: "MyUnbelievableNamespace")]

namespace CustomRegistrationMethodName;

[RegisterAsSelf]
[Scoped]
public class AliasedAssemblyService
{
}
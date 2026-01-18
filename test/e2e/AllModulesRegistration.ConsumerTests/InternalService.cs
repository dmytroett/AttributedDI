using AttributedDI;

namespace AllModulesRegistration.ConsumerTests;

[RegisterAsGeneratedInterface]
public partial class InternalService
{
    public void ThisIsInternal()
    {
    }
}

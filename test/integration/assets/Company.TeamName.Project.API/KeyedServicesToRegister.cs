using AttributedDI;

namespace Company.TeamName.Project.API;

// Keyed services
public interface IKeyedService
{
}

[RegisterAs<IKeyedService>("key1")]
[Singleton]
public class KeyedServiceOne : IKeyedService
{
}

[RegisterAs<IKeyedService>("key2")]
[Singleton]
public class KeyedServiceTwo : IKeyedService
{
}

[RegisterAsSelf("transientKey")]
public class RegisterAsSelfKeyedTransientService
{
}

[RegisterAsSelf("singletonKey")]
[Singleton]
public class RegisterAsSelfKeyedSingletonService
{
}
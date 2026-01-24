# AttributedDI

Keep DI registration close to your services.

AttributedDI lets you mark services with simple attributes, generates interfaces from your
implementations, and wires everything with the equivalent
`services.AddTransient/Scoped/Singleton(...)` calls at compile time. Registrations live
next to the types they describe, and the generator writes the wiring for you (no runtime
reflection, AOT friendly).

## Why

If you've ever shipped a bug because you forgot to register a service in `Program.cs`,
or spent time debating lifetimes while staring at a giant registration list, this is for you.
I built AttributedDI after repeatedly:

- forgetting to add new services in `Program.cs`,
- second-guessing the correct lifetime for each service, and
- refactoring `Program.cs` as the list grew past a dozen registrations.

Manual DI registration scales poorly as projects grow, and runtime scanning is slow and
unfriendly to trimming/AOT. AttributedDI keeps registration colocated with the type, and
its interface generation helps you introduce abstractions without the usual manual churn.
It produces normal, readable registration code during build.

## Features

- Interface generation from concrete types (with optional auto-registration).
- Attribute-driven registration for self, implemented interfaces, or a specific service type.
- Compile-time source generation (no runtime reflection scan).
- Keyed registrations when you pass a key to registration attributes.
- Optional interface generation from concrete types.
- Customizable extension class/method names.
- Optional aggregate `AddAttributedDi()` to register across references.

## Installation

Add the package to any project that defines services:

```bash
dotnet add package AttributedDI
```

The source generator is included automatically with the package reference.

## Quick start

Annotate services and call the generated extension method.

```csharp
using AttributedDI;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

public interface IClock
{
    DateTime UtcNow { get; }
}

[Singleton]
[RegisterAs<IClock>]
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

[Scoped]
[RegisterAsSelf]
public sealed class Session
{
}
```

AttributedDI can also generate an interface for you and register against it in one step:

```csharp
[RegisterAsGeneratedInterface]
public sealed class MetricsSink
{
    public void Write(string name, double value) { }

    [ExcludeInterfaceMember]
    public string DebugOnly => "local";
}
```

At build time, AttributedDI generates an extension method named `Add{AssemblyName}` on
`{AssemblyName}ServiceCollectionExtensions` (names are sanitized into valid identifiers).
Use it in startup:

```csharp
var services = new ServiceCollection();
services.AddMyApp();
```

## Registration options

- `RegisterAsSelf` registers the type as itself.
- `RegisterAsImplementedInterfaces` registers the type against all implemented interfaces.
- `RegisterAs<TService>` registers the type against a specific service type.
- Apply `Transient`, `Scoped`, or `Singleton` to pick a lifetime (default is transient).
- Pass a key to any registration attribute to generate keyed registrations.

Example with a keyed registration:

```csharp
[Singleton]
[RegisterAs<IGateway>("primary")]
public sealed class PrimaryGateway : IGateway
{
}
```

## Interface generation

Generate an interface from a concrete type:

```csharp
[GenerateInterface]
public sealed class WeatherClient
{
    public Task<string> GetAsync(string city, CancellationToken ct) => Task.FromResult("ok");
}
```

Generate an interface and register against it in one step:

```csharp
[RegisterAsGeneratedInterface]
public sealed class MetricsSink
{
    public void Write(string name, double value) { }

    [ExcludeInterfaceMember]
    public string DebugOnly => "local";
}
```

For edge cases and exclusions, see `docs/interface-generation-exceptions.md`.

## Customize the generated extension

If you want stable, explicit names, apply the assembly-level attribute:

```csharp
using AttributedDI;

[assembly: ServiceCollectionExtension(
    extensionClassName: "DependencyInjectionExtensions",
    methodName: "AddMyServices",
    extensionNamespace: "MyApp")]
```

Then call:

```csharp
services.AddMyServices();
```

## Aggregate registration across references

If you have multiple projects that each define services, you can generate a single
`AddAttributedDi()` method in the top-level project that calls all of their generated
extension methods.

Typical setup:

1. Install `AttributedDI` in each downstream project that defines services.
2. Mark services with registration attributes in those downstream projects.
3. Reference those downstream projects from your upstream (composition root) project.
4. Enable aggregate generation in the upstream project.

Enable it in the upstream project file:

```xml
<PropertyGroup>
  <GenerateAttributedDIExtensions>true</GenerateAttributedDIExtensions>
</PropertyGroup>
```

Then call:

```csharp
services.AddAttributedDi();
```

## License

MIT. See `LICENSE`.

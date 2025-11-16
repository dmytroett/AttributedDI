# Plan: Migrate AttributedDI to Source Generators with AOT Support

Migrate from runtime reflection to compile-time source generation targeting .NET 8.0. Generate per-assembly registration methods that allow consumers to selectively register services from specific assemblies via `AddServicesFromAssemblies(params Assembly[] assemblies)`, providing flexibility for multi-project solutions and multiple entry points. Use cached attribute metadata via `ForAttributeWithMetadataName` for optimal incremental build performance.

## Steps

1. Create `AttributedDI.SourceGenerator` project targeting `netstandard2.0` with `Microsoft.CodeAnalysis.CSharp` 4.8.0+ package, implement `IIncrementalGenerator` using `ForAttributeWithMetadataName` pipeline to discover all types with `RegisterBase`-derived attributes with cached metadata for optimal performance

2. Update [`AttributedDI.csproj`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\AttributedDI.csproj) to target `net8.0` with `IsAotCompatible` and `IsTrimmable` properties, convert attributes to sealed marker classes removing `PerformRegistration` methods from [`RegisterBase.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegisterBase.cs), [`RegisterAsAttribute.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegisterAsAttribute.cs), [`RegisterAsSelfAttribute.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegisterAsSelfAttribute.cs), and [`RegisterAsImplementedInterfacesAttribute.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegisterAsImplementedInterfacesAttribute.cs)

3. Generate `AddServicesFrom{AssemblyName}()` extension methods in `AttributedDI` namespace for each assembly containing attributed types, plus `AddServicesFromAssemblies(params Assembly[] assemblies)` method in [`RegistrationServiceCollectionExtensions.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegistrationServiceCollectionExtensions.cs) that dispatches to assembly-specific generated methods based on assembly identity

4. Add diagnostic analyzers with error codes: `ATDI001` for `RegisterAsImplementedInterfacesAttribute` on types without interfaces, `ATDI002` for `RegisterAsAttribute` with incompatible service types (not assignable), `ATDI003` for abstract/static types with registration attributes, `ATDI004` for duplicate service+implementation+lifetime combinations

5. Delete [`AssemblyScanner.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\AssemblyScanner.cs), remove old assembly-scanning methods from [`RegistrationServiceCollectionExtensions.cs`](C:\Users\Personal\Documents\Repositories\AttributedDI\src\AttributedDI\RegistrationServiceCollectionExtensions.cs), update all test projects to `net8.0`, replace `AddServicesFromAssemblyContainingType<T>()` calls with new `AddServicesFromAssemblies()` API, add snapshot tests for generator output and AOT compatibility validation

## Further Considerations

1. **Assembly identity matching**: Use `Assembly.FullName` or `Assembly.GetName().Name` for matching assemblies in `AddServicesFromAssemblies()` dispatcher - consider version tolerance?

2. **Unknown assembly handling**: Should `AddServicesFromAssemblies()` silently skip assemblies without generated registration methods, or throw exception for better developer feedback?

3. **Convenience overloads**: Add `AddServicesFromAssemblyContaining<T>()` and `AddServicesFromAssemblyContaining(Type type)` convenience methods that call `AddServicesFromAssemblies(typeof(T).Assembly)`?


# Codex Instructions for AttributedDI

## Overview

This project is about building a library to simplify dependency registration in DI container. The general idea - mark components that need to be registered with an attribute, like `[RegisterAsSelf]`, and the library will  generate the code like `services.AddTransient<MyService>()` to automatically register and wire all of the components. Heavily uses .NET source generators to avoid reflection scan at runtime.

## General Guidelines

- When uncertain about APIs, best practices, or modern implementation patterns, consult Microsoft documentation.
- When usage is unclear, search GitHub source code for referenced open source projects.
- Prefer the configured MCP servers (GitHub, Microsoft Docs) as your first reference sources.

## Code Quality Guidelines

- Make sure code is maintainable and easy to understand. Suggest refactoring when beneficial.
- If refactoring is challenging, complicated, or the user explicitly declined, add strategic comments to improve maintainability instead.
- Do not overuse comments; place them only where they add real value.
- Minimize public surface area: Use `private` or `internal` access modifiers by default unless the API is intentionally designed to be public. A smaller public API is easier to maintain and reduces breaking change concerns in future versions.

## Code Style & Formatting

When done with code generation or modification, you must:

1. Run `dotnet format --no-restore --include <list-of-changed-files>`
2. Ensure no formatting issues remain

Example:
```
dotnet format --no-restore --include src/AttributedDI/MyClass.cs
```

For multiple files:
```
dotnet format --no-restore --include src/AttributedDI/File1.cs src/AttributedDI/File2.cs
```

## Public API Documentation

This is a library. **All** public methods, properties, classes, and interfaces in library code **must** have XML documentation (///).

This requirement applies to library code only (`src/AttributedDI/`, `src/AttributedDI.SourceGenerator/`). Test projects are exempt.

Document only public APIs in library projects

Update documentation when signatures change

### Example

```csharp
/// <summary>Registers a service with the DI container.</summary>
/// <param name="serviceType">The service type to register.</param>
/// <param name="implementationType">The implementation type.</param>
/// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
public void RegisterService(Type serviceType, Type implementationType)
```

### Documentation Rules

- Document only public APIs in library projects
- Update documentation when signatures change

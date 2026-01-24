# Codex Instructions for AttributedDI

## Overview

This project is about building a library to simplify dependency registration in DI container. The general idea - mark components that need to be registered with an attribute, like `[RegisterAsSelf]`, and the library will generate the code like `services.AddTransient<MyService>()` to automatically register and wire all of the components. Heavily uses .NET source generators to avoid reflection scan at runtime.

## General Guidelines

- For any Source Generator task, always consult Microsoft Docs via the Microsoft Learn MCP first and treat it as the source of truth before reasoning or coding.
- For anything involving GitHub, always use the GitHub MCP (not web search) to interface with GitHub, and proactively use code search, issues, and PRs when helpful.

## Code Quality Guidelines

- Make sure code is maintainable and easy to understand. Suggest refactoring when beneficial.
- If refactoring is challenging, complicated, or the user explicitly declined, add strategic comments to improve maintainability instead.
- Do not overuse comments; place them only where they add real value.
- Minimize public surface area: Use `private` or `internal` access modifiers by default unless the API is intentionally designed to be public. A smaller public API is easier to maintain and reduces breaking change concerns in future versions.

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

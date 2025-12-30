# GitHub Copilot Instructions for AttributedDI

## General Guidelines

- **Always consult Microsoft documentation** when uncertain about APIs, best practices, or modern implementation patterns.
- **Search GitHub source code** for referenced open source projects if usage is unclear.

## Code Quality Guidelines

- Make sure code is maintainable and easy to understand. Suggest refactoring when beneficial.
- If refactoring is challenging, complicated, or user explicitly declined - add strategic comments to improve maintainability instead (even for private/internal code)
- Do not overuse comments - place them strategically where they add the most value

## Code Style & Formatting

When done with code generation or modification, you MUST:

1. Use the `run_in_terminal` tool to execute: `dotnet format --include <list-of-changed-files>`
2. Wait for the formatting command to complete
3. Use the `get_errors` tool to verify no formatting issues remain

Example command format:
```
dotnet format --include src/AttributedDI/MyClass.cs
```

For multiple files:
```
dotnet format --include src/AttributedDI/File1.cs src/AttributedDI/File2.cs
```

## Public API Documentation

This is a library. **ALL** public methods, properties, classes, and interfaces in library code **MUST** have XML documentation (///).

> **Scope**: This requirement applies to library code only (`src/AttributedDI/`, `src/AttributedDI.SourceGenerator/`). Test projects are exempt.

### Required XML Tags

- `<summary>`: What it does
- `<param>`: Each parameter's purpose and constraints
- `<returns>`: Return value description
- `<exception>`: Any exceptions thrown
- `<example>`: Usage examples (for complex/frequently-used APIs)
- `<remarks>`: Edge cases or important notes (when needed)

### Example

```csharp
/// <summary>Registers a service with the DI container.</summary>
/// <param name="serviceType">The service type to register.</param>
/// <param name="implementationType">The implementation type.</param>
/// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
public void RegisterService(Type serviceType, Type implementationType)
```

### Documentation Rules

- Document **only** public APIs in library projects
- Update documentation when signatures change

## Implementation Process

For new features or significant changes:

1. **Present a Plan**: Short, high-level plan with classes/interfaces, their responsibilities, and key decisions
2. **Wait for Approval**: Proceed only after user confirmation
3. **Implement**: Follow the approved plan
4. **Format**: Run `dotnet format` on changed files
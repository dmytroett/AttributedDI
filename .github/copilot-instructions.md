# GitHub Copilot Instructions for AttributedDI

## Public API Documentation

This is a library. **ALL** public methods, properties, classes, and interfaces **MUST** have XML documentation (///).

### Required Tags

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

### Guidelines

- Document ONLY public code. 
- Make sure the code is maintainable and easy to understand. Suggest refactoring the code for that purpose.
- If the refactoring is challenging, complicated or user deliberately asked to not refactor - add comments and document such code instead even if it is private/internal. Do not overuse comments, place comments strategically to improve maintainability.
- Update docs when signatures change
- Query Microsoft docs to fetch relevant up-to-date documentation

### Code Style & Formatting

**MANDATORY**: When done with code generation or modification, you MUST:

1. Use the `run_in_terminal` tool to execute: `dotnet format --include <list-of-changed-files>`
2. Wait for the formatting command to ~~~~complete
3. Use the `get_errors` tool to verify no formatting issues remain

Example command format:
```
dotnet format --include src/AttributedDI/MyClass.cs
```

For multiple files:
```
dotnet format --include src/AttributedDI/File1.cs src/AttributedDI/File2.cs
```

**DO NOT** skip this step. Formatting is not optional and must be performed before completing your turn.

## Implementation Process

When implementing new features or significant changes:

1. **Present a Plan First**: Before writing code, provide a short, high-level plan including:
   - Which classes/interfaces will be created
   - Their responsibilities (if not obvious from the name)
   - Key architectural decisions
   - No need to list methods or detailed implementation

2. **Wait for Approval**: Proceed with implementation only after the user gives the green light

3. **Implement**: Follow through with the approved plan

4. **Format Code**: Run `dotnet format` on all changed files (see Code Style & Formatting section above)

### Example Plan Format

```
Plan:
- Create `IServiceRegistry` interface - manages service registration lifecycle
- Create `ServiceRegistrar` class - implements registration logic
- Modify `RegistrationExtensions` - add new extension methods for the feature

Proceed with implementation?
```


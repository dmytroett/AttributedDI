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

- Internal methods: document only if complex
- Update docs when signatures change
- Query Microsoft docs to fetch relevant up-to-date documentation

### Code Style

- After generating or modifying code, automatically run `dotnet format` to adhere to `.editorconfig` rules
- Only format files that have been changed and contain relevant `.editorconfig` rules

## Implementation Process

When implementing new features or significant changes:

1. **Present a Plan First**: Before writing code, provide a short, high-level plan including:
   - Which classes/interfaces will be created
   - Their responsibilities (if not obvious from the name)
   - Key architectural decisions
   - No need to list methods or detailed implementation

2. **Wait for Approval**: Proceed with implementation only after the user gives the green light

3. **Implement**: Follow through with the approved plan

### Example Plan Format

```
Plan:
- Create `IServiceRegistry` interface - manages service registration lifecycle
- Create `ServiceRegistrar` class - implements registration logic
- Modify `RegistrationExtensions` - add new extension methods for the feature

Proceed with implementation?
```


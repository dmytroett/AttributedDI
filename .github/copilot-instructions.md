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
- Keep concise but complete
- Update docs when signatures change

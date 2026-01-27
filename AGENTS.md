# Codex Instructions for AttributedDI

## Overview

Library that uses .NET source generators to turn attributes (e.g. `[RegisterAsSelf]`) into DI registrations (e.g. `services.AddTransient<MyService>()`) without runtime reflection scanning.

## General Guidelines

- Source generators: consult Microsoft Learn MCP first (source of truth) any time working with source generators.
- GitHub work: use the GitHub MCP (not web search).
- Functional changes (new/changed/removed behavior or public API): add an entry to `CHANGELOG.md` under `[Unreleased]` using the template below. Breaking changes go under `### Breaking Changes`, start with `BREAKING:`, and include a brief migration note.

### CHANGELOG.md Template

Under `## [Unreleased]`, use this structure (omit empty sections):

```md
### Breaking Changes

- BREAKING: <what changed>. Migration: <how to update>.

### Added

- <new functionality>.

### Changed

- <behavior change>.

### Deprecated

- <deprecated behavior/API>.

### Removed

- <removed functionality/API>.

### Fixed

- <bug fix>.

### Security

- <security-related change>.
```

## Code Quality Guidelines

- Prefer maintainable, easy-to-read code; refactor when it helps.
- If refactoring is too costly or declined, add small, high-value comments (don't over-comment).
- Minimize public surface area: prefer `internal`/`private` unless intentionally public API.

## Public API Documentation

Library projects require XML docs (`///`) on all public APIs:

- Applies to `src/AttributedDI/` and `src/AttributedDI.SourceGenerator/`.
- Test projects are exempt.
- Update docs when signatures change.

### Example

```csharp
/// <summary>Registers a service with the DI container.</summary>
/// <param name="serviceType">The service type to register.</param>
/// <param name="implementationType">The implementation type.</param>
/// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
public void RegisterService(Type serviceType, Type implementationType)
```

# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

## [1.0.1] - 2026-01-27

- Updated the README to clarify that `[GenerateInterface]` and `[RegisterAsGeneratedInterface]` require non‑nested partial classes or structs.

## [1.0.0] - 2026-01-27

- Attribute-driven registrations (`RegisterAsSelf`, `RegisterAsImplementedInterfaces`, `RegisterAs<TService>`).
- Lifetime selection via `Transient`, `Scoped`, and `Singleton` (transient default).
- Keyed registrations via attribute key arguments.
- Interface generation with `GenerateInterface` and `RegisterAsGeneratedInterface`.
- Interface member exclusions via `ExcludeInterfaceMember`.
- Custom generated extension naming via `ServiceCollectionExtension`.
- Aggregate `AddAttributedDi()` generation across referenced projects (opt-in MSBuild property).
- Source-generated registration code (no runtime reflection scanning).

## [1.0.0-alpha001] - 2026-01-24

- Attribute-driven registrations (`RegisterAsSelf`, `RegisterAsImplementedInterfaces`, `RegisterAs<TService>`).
- Lifetime selection via `Transient`, `Scoped`, and `Singleton` (transient default).
- Keyed registrations via attribute key arguments.
- Interface generation with `GenerateInterface` and `RegisterAsGeneratedInterface`.
- Interface member exclusions via `ExcludeInterfaceMember`.
- Custom generated extension naming via `ServiceCollectionExtension`.
- Aggregate `AddAttributedDi()` generation across referenced projects (opt-in MSBuild property).
- Source-generated registration code (no runtime reflection scanning).


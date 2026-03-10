# Compile-Time DI Scope

This document defines the shared scope across all design options.

## Problem statement

AttributedDI currently focuses on source-generated registrations. The next step is a source-generated `IServiceProvider` path that avoids runtime reflection/scanning and reduces runtime resolution overhead.

## Goals

- Keep the integration model familiar for `Microsoft.Extensions.DependencyInjection` (MEDI) users. And preserving maximum compatibility with `IServiceProvider` interface to make it compatible with current projects. Ideally it should be a drop-in replacemet, however I can drop compatibility with some feature if it yields significant performance improvement is not used much.
- Support multi-assembly scenarios where each assembly can contribute generated provider parts.
- Preserve fallback behavior to runtime registrations when a service is not compile-time registered.

## Non-goals for V1

- Replacing MEDI contracts with a custom container API.

## Registration input models

Two registration styles are in scope:

1. Centralized provider-part attributes.
2. Attributes directly on service types.

Both styles can emit the same generated provider part metadata, so teams can mix them in one solution.

## Decision criteria

- Runtime throughput and allocations.
- Startup/JIT cost.
- Compatibility with MEDI behaviors.
- Generated code size and readability.
- Cross-assembly composition complexity.

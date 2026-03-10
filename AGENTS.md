# Codex Instructions for AttributedDI

## Overview

Library that uses .NET source generators to turn attributes (e.g. `[RegisterAsSelf]`) into DI registrations (e.g. `services.AddTransient<MyService>()`) without runtime reflection scanning.

## General Guidelines

- Source generators: consult Microsoft Learn MCP first (source of truth) any time working with source generators.
- GitHub work: use the GitHub MCP (not web search).
- Functional changes (new/changed/removed behavior or public API): add one short bullet to `CHANGELOG.md` under `## [Unreleased]` describing what changed.
- Breaking changes: prefix the bullet with `**BREAKING:**` and optionally include a migration note when there is a clear migration path.

## Code Quality Guidelines

- Prefer maintainable, easy-to-read code; refactor when it helps.
- If refactoring is too costly or declined, add small, high-value comments (don't over-comment).

---
name: snapshot-unit-testing
description: Set of principles for unit and snapshot testing. Use when adding/fixing unit tests or when working with `Verify` snapshots. Do not load when working with integration or e2e tests.
---

## Scope

- Unit tests live under `test/*UnitTests`.
- Follow these instructions when writing/updating **unit** tests. Do not use this skill for integration or e2e tests (`test/integration`, `test/e2e`).

## Principles

- Use `Verify` snapshot testing tool for generated output (source generated code, files, etc). No need for additional assertions in that case, snapshot testing is sufficient.
- For normal assertions, prefer small, domain-specific assertion helpers over chaining multiple `Assert.*` calls. Example: `DiagnosticAssert.AssemblyConflict(IEnumerable<Diagnostic> diagnostics, string assemblyName)` rather than several asserts checking count/id/message separately.
- Prefer a small number of representative scenarios; combine related assertions to keep snapshot sprawl low.
- Run targeted tests with `dotnet test --filter "FullyQualifiedName~<TestClass>.<TestMethod>"` (add `--no-build --no-restore` when appropriate).
- Do not open `.received` / `.verified` files. `dotnet test` command output includes enough information to conclude whether there is a bug in the code, test logic is wrong or snapshot need to be updated (accepted).
- Accept snapshots only when the behavior change is intended and understood.
- Do not inspect or edit the accept scripts (treat them as black boxes).

## Accepting Snapshots

- One test: `.codex/skills/snapshot-unit-testing/scripts/accept-snapshot.ps1 <TestClassName> <TestMethodName>`
- All tests: `.codex/skills/snapshot-unit-testing/scripts/accept-all-snapshots.ps1`
- Re-run corresponding tests after accepting snapshots to ensure they pass.

## Recognizing a Verify Failure

Every time there is a mismatch between received/verified files following will be included in `dotnet test` output:

```
VerifyException : Directory: /home/me/projects/AttributedDI/test/AttributedDI.SourceGenerator.UnitTests/Snapshots
  NotEqual:
    - Received: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.DotNet9_0.received.txt
      Verified: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.verified.txt
```

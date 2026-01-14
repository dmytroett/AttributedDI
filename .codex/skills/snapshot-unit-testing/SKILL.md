---
name: snapshot-unit-testing
description: Write or update unit (not integration) tests in this repo using Verify snapshot testing, especially for source generator output, and run/accept snapshots via the provided scripts.
---

# Scope

Locate unit tests under `test/*UnitTests`. Use Verify snapshots when validating generated output; prefer normal assertions for non-generator logic.

# Principles

Favor a small number of representative scenarios; combine related assertions to minimize snapshot files even if individual tests are larger.

Avoid opening or reading `.received`/`.verified` files; rely on `dotnet test` output and VerifyException details only.

Treat `scripts/accept-snapshot` and `scripts/accept-all-snapshots` as black boxes; do not open or inspect them.

# Workflow

Write or update the test based on a concrete scenario.

Run targeted tests with `dotnet test --filter` and add `--no-build`/`--no-restore` when appropriate.

Inspect `dotnet test` output to decide whether the test logic is wrong or snapshots need updating; do not open raw snapshot files.

Accept snapshots only when the behavior change is intended and clearly understood:
- Use `scripts/accept-snapshot <TestClassName> <TestMethodName>` for a single test.
- Use `scripts/accept-all-snapshots` for bulk updates.
- Skip these scripts for non-snapshot tests.

Re-run tests and iterate until they pass.

# Commands

- Run a single test:
  - `dotnet test --filter "FullyQualifiedName~<TestClass>.<TestMethod>"`
- Accept one snapshot:
  - `scripts/accept-snapshot <TestClassName> <TestMethodName>`
- Accept all snapshots:
  - `scripts/accept-all-snapshots`

# Verify failure pattern

Use the failure output only; do not open the files listed.

```
VerifyException : Directory: /home/me/projects/AttributedDI/test/AttributedDI.SourceGenerator.UnitTests/Snapshots
  NotEqual:
    - Received: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.DotNet9_0.received.txt
      Verified: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.verified.txt
```

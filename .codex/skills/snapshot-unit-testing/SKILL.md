---
name: snapshot-unit-testing
description: Workflow for snapshot-based unit tests (Verify or similar) under test/*UnitTests; run filtered tests, validate analyzer warnings and source generator output when applicable, accept snapshots only when relevant, and iterate until passing.
---

# Snapshot Unit Testing (Verify)

Use this skill when adding or modifying unit tests under `test/*UnitTests` that use Verify snapshots.
If a test does not use snapshots, skip snapshot acceptance steps and follow the appropriate assertion workflow instead.

## Workflow

1. **Test intent**: Ensure the test asserts both:
   - Generated analyzer warnings/issues.
   - Source generator output.
   If the test is not snapshot-based, keep this verification but do not use snapshot acceptance scripts.
2. **Run targeted test**:
   - Use `dotnet test` with `--filter` to run only the relevant test(s).
   - Consider `--no-build` and `--no-restore` when appropriate.
3. **Inspect results**:
   - Read the output and infer whether the test logic is wrong or snapshots need updating.
4. **Accept snapshots when correct** (snapshot tests only):
   - Use `scripts/accept-snapshot` for a single test, or `scripts/accept-all-snapshots` for bulk updates.
   - If the test is not snapshot-based, do not use these scripts.
5. **Iterate** until all relevant tests pass.

## Commands

- Single test:
  - `dotnet test --filter "FullyQualifiedName~<TestClass>.<TestMethod>"`
- Accept one snapshot:
  - `scripts/accept-snapshot <TestClassName> <TestMethodName>`
- Accept all snapshots:
  - `scripts/accept-all-snapshots`

## Failure pattern (Verify)

Typical Verify failures list a received file and a verified file, for example:

```
VerifyException : Directory: /home/me/projects/AttributedDI/test/AttributedDI.SourceGenerator.UnitTests/Snapshots
  NotEqual:
    - Received: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.DotNet9_0.received.txt
      Verified: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.verified.txt
```

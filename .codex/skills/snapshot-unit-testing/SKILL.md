---
name: snapshot-unit-testing
description: Write/update unit tests; use Verify snapshots for source generator output; accept snapshots via the provided scripts.
---

# Snapshot Testing (Verify)

Follow these instructions when writing/updating **unit** tests in this repo, especially tests that validate source generator output via Verify snapshots.

## Defaults

- Unit tests live under `test/*UnitTests`.
- Use Verify snapshots for generated output; prefer normal assertions for non-generator logic.

## Do

- Prefer a small number of representative scenarios; combine related assertions to keep snapshot sprawl low.
- Run targeted tests with `dotnet test --filter "FullyQualifiedName~<TestClass>.<TestMethod>"` (add `--no-build --no-restore` when appropriate).
- If Verify fails, decide “test logic wrong” vs “snapshots need update” using `dotnet test` output only.

## Don't

- Do not open `.received` / `.verified` files; rely on `dotnet test` output (including `VerifyException`) only.
- Accept snapshots only when the behavior change is intended and understood.
- Do not inspect or edit the accept scripts (treat them as black boxes).

## Accept Snapshots

- One test: `.codex/skills/snapshot-unit-testing/scripts/accept-snapshot.ps1 <TestClassName> <TestMethodName>`
- All tests: `.codex/skills/snapshot-unit-testing/scripts/accept-all-snapshots.ps1`

## Recognizing a Verify Failure

Use the failure output only; do not open the files listed.

```
VerifyException : Directory: /home/me/projects/AttributedDI/test/AttributedDI.SourceGenerator.UnitTests/Snapshots
  NotEqual:
    - Received: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.DotNet9_0.received.txt
      Verified: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.verified.txt
```

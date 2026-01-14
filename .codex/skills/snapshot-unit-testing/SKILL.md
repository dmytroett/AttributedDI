---
name: snapshot-unit-testing
description: Write unit tests for the project. Use this skill each time the user wants to add/edit unit (but not integration) tests in the project.
---

# Overview

Unit tests are located under `test/*UnitTests`. Most of the tests should be snapshot based, e.g. there is a snapshot of an output from the previous run of the test, subsequent runs compare new snapshot with the old one. The difference between verified and received is interpreted by test runner as a bug.

Never read raw snapshot files. Use test output (TODO expand on that).

Not everything should be tested via snapshot testing approach. Use snapshot based approach only when needed to verify source generator output.

Come up with particular usage scenarios and align tests accordingly. Prefer small amount of independend scenarios (consequently small amout of tests) even if that means individual tests can be large. Such approach leads to smaller amount of snapshot files, which makes it easier to manage. Suggest restructuring/regrouping/realigning existing tests to match this recommendation.

**Run targeted test**:
   - Use `dotnet test` with `--filter` to run only the relevant test(s).
   - Consider `--no-build` and `--no-restore` when appropriate.

**Inspect results**:
   - Use `dotnet test` output (failure summary, VerifyException details, and test names) to infer whether the test logic is wrong or snapshots need updating.
   - Base the decision on the test name and failure reason; do not open or read raw snapshot `.received`/`.verified` files.   

**Accept snapshots when correct** (snapshot tests only):
   - Use `scripts/accept-snapshot` for a single test, or `scripts/accept-all-snapshots` for bulk updates.
   - If the test is not snapshot-based, do not use these scripts.
   - Do not open or read script files. Use them assuming as black box.
   - Avoid accepting snapshots if the behavior change is unintended or the test intent is unclear. Ask the user for clarification if needed.

Use `Verify` library for snapshot testing. Iterate until all tests pass.

# Commands

- Single test:
  - `dotnet test --filter "FullyQualifiedName~<TestClass>.<TestMethod>"`
- Do not open snapshot files:
  - Avoid reading `.received`/`.verified` files; rely on `dotnet test` output instead.
- Accept one snapshot:
  - `scripts/accept-snapshot <TestClassName> <TestMethodName>`
- Accept all snapshots:
  - `scripts/accept-all-snapshots`

## Failure pattern (Verify)

Typical Verify failures list a received file and a verified file, for example (use this output only, do not read the files):

```
VerifyException : Directory: /home/me/projects/AttributedDI/test/AttributedDI.SourceGenerator.UnitTests/Snapshots
  NotEqual:
    - Received: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.DotNet9_0.received.txt
      Verified: AddAttributedDiTests.GeneratesAddAttributedDiForEntryPoint.verified.txt
```

---
name: version-analyzer-releases
description: Tracks and makes sure that all changes to roslyn analyzers are documented and versioned correctly. Use each time when adding/changing (severity, category, etc)/deleting a roslyn analyzer.
---

# Version Analyzer Releases

Update `src/AttributedDI.SourceGenerator/AnalyzerReleases.Unshipped.md` for every analyzer change (add/remove/change). "Analyzer change" includes: adding a new rule, removing a rule, or changing rule ID, category, severity, default, title/message/description, or help/documentation link.

- Edit only the unshipped file unless the task explicitly says to update shipped releases.
- Keep the exact section headers and table headers as they are in the file.
- Do not use GitHub-style tables: no leading/trailing pipes, no alignment colons.
- Preserve the dashed separator row style exactly.
- Rule IDs must start with ATTDI (for example ATTDI001, ATTDI002).
- New analyzer: add a row under “New Rules.”
- Removed analyzer: add a row under “Removed Rules.”
- Changed category/severity/notes: add a row under “Changed Rules,” filling both new and old columns.
- Sort rows by Rule ID within each section (the correct section is more important than ordering).
- Notes should be a short summary of the rule. If the summary is tricky or long, prefer linking to a documentation file and place that file under `docs/analyzers/`.
- If multiple changes happen to the same rule while unshipped, keep a single row that reflects the final state. The last change wins; e.g., a changed-then-removed rule should appear only under “Removed Rules.”

An example of each section filled properly:

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
ATTDI001 | Design | Warning | ATTDI001_AnalyzerName, [Documentation](ATTDI001_Documentation_Link)

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
ATTDI002 | Usage | Hidden | ATTDI002_AnalyzerName, [Documentation](ATTDI002_Documentation_Link)

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
ATTDI003 | Security | Hidden | Security | Info | ATTDI003_AnalyzerName, [Documentation](ATTDI003_Documentation_Link)

---
name: version-analyzer-releases
description: Tracks and makes sure that all changes to roslyn analyzers are documented and versioned correctly. Use each time when adding/changing (severity, category, etc)/deleting a roslyn analyzer.
---

# Analyzer Releases Unshipped

## Overview

Keep `src/AttributedDI.SourceGenerator/AnalyzerReleases.Unshipped.md` in sync with analyzer changes by adding, removing, or updating rows in the correct section and format.

## Workflow

1. Identify analyzer changes (added, removed, or changed category/severity/notes).
2. Select the correct section: New Rules, Removed Rules, or Changed Rules.
3. Add or update table rows using the exact headers and separator format.
4. Keep notes optional and concise.

## Formatting Rules

- Use the exact section headers and table headers from the current file.
- Do not use GitHub-style tables: no leading or trailing pipes, no alignment colons.
- Preserve the dashed separator row style (see reference file).
- Notes may be empty; leave the Notes cell blank after the final pipe.

## Entry Guidance

- New analyzer -> add a row under New Rules.
- Removed analyzer -> add a row under Removed Rules.
- Category or severity change -> add a row under Changed Rules with both new and old values.
- If multiple analyzers change, add multiple rows. Keep ordering consistent with the existing file.
- Notes are arbitrary and optional; they do not need to link to documentation.

## Resources

Use `assets/analyzer-releases-unshipped-format.md` as the canonical formatting template.

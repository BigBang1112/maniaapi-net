---
name: release-notes
description: Prepare .NET package versions, concise Markdown release notes, and release PR updates. Use when preparing a release, updating a PR's release notes, or fixing formatting between PackageReleaseNotes and GitHub releases.
---

# Release notes

## Identify affected packages and bump versions

- Inspect the complete PR diff against its merge base, or changes since the previous relevant release for release-wide work. Map changed files to the packages that compile, generate, or pack them, including shared files and build configuration.
- Bump every package whose shipped content or metadata changes. Include dependent packages when their packed dependency versions change. Test-only or tooling-only changes need no bump unless they affect the package.
- Edit the existing source of the effective `PackageVersion`, including imported properties or `VersionPrefix`/`VersionSuffix` where used. Retain a suitable unpublished bump already present in the PR. Never reuse a published version for changed content.
- Follow the requested version or repository policy. Otherwise use patch for compatible fixes, minor for compatible features, and major for breaking changes, respecting prerelease and pre-1.0 conventions. Keep independently versioned packages independent.

## Write the notes

- Describe verified consumer-facing changes in compact `- ` bullets, using concrete verbs such as "Added", "Fixed", "Updated", or "Removed". Name the affected API and explain the practical effect.
- Use backticks for identifiers and literal markup, selective bold for major additions or breaking changes, and nested bullets for related details. Put major features first, then behavior changes and fixes. Add categories only when useful.
- Explain breaking changes and their replacements. Avoid promotional language, vague cleanup entries, semicolons, and em dashes.
- Keep notes scoped to the pending version. Extend and deduplicate existing notes for that release, replacing notes from the previous published version. Use absolute URLs for useful links.

## Preserve Markdown through publishing

Inspect the actual publishing workflow, including external actions or scripts, to identify its note source, property overrides, formatting transformations, and generated headings. Keep notes in the repository's existing source of truth.

For notes stored in project metadata, use multiline `PackageReleaseNotes`. Keep top-level bullets at column zero unless the workflow removes common indentation. Preserve the surrounding file's formatting.

```xml
        <PackageReleaseNotes>
- **Added `ExampleClient`** for server notifications
  - Added `OnNotification` to receive updates
- Fixed parsing of `&lt;value&gt;` responses containing `&amp;`
        </PackageReleaseNotes>
```

- Escape XML once while retaining literal newlines, backticks, bold markers, and relative indentation. Do not flatten Markdown or replace newlines with literal `\n`. Check MSBuild expressions and escapes in literal examples against the evaluated output.
- Do not duplicate package headings or footers supplied by the workflow. Remove only whitespace the workflow is intended to remove, never trim each line.
- Evaluate `PackageId`, `PackageVersion`, and `PackageReleaseNotes` with `dotnet msbuild -getProperty:PackageId,PackageVersion,PackageReleaseNotes` for each affected project, using publish-time options. Decode the JSON and check the actual strings.
- Preview the assembled release Markdown without publishing. When changing packaging, version propagation, or note transport, inspect the packed `.nuspec` for versions, dependency ranges, and decoded `releaseNotes`.

## Update the PR

When requested, apply the changes on the PR's branch and update its release-notes section with the effective package versions and matching decoded note bodies. Preserve useful existing description content and replace stale entries rather than appending duplicates.

Use structured API arguments or `gh pr edit --body-file` to preserve Markdown, then read back the description. Follow existing commit and push instructions. Preparing notes does not itself request merging, tagging, or publishing.

Report the affected versions, validation results, and updated PR link.

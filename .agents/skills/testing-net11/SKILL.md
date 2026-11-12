---
name: testing-net11
description: Run or configure ManiaAPI.NET's TUnit tests with .NET 11 Microsoft.Testing.Platform features. Use for SDK 11 test commands, framework selection, or runner diagnostics; retain the repository's live-test opt-in.
---

# Testing with .NET 11

Check `dotnet --version`, [global.json](../../../global.json), and [Tests/Directory.Build.props](../../../Tests/Directory.Build.props). The repository selects Microsoft.Testing.Platform (MTP), uses TUnit, and targets .NET 8 through 11. SDK version determines CLI features; `-f net11.0` selects a target but does not install or select an SDK.

Use explicit MTP project/solution options and pass TUnit filters directly, without the legacy `--` separator:

```powershell
dotnet test --project Tests/ManiaAPI.NadeoAPI.Tests/ManiaAPI.NadeoAPI.Tests.csproj -f net11.0
dotnet test --solution ManiaAPI.NET.slnx -f net11.0
```

For a subset, use TUnit's `--treenode-filter`; confirm filter syntax with the installed test application's help. Reuse the [live workflow's](../../../.github/workflows/live-tests.yml) category filter when live tests are requested. Ordinary runs keep live tests disabled through [LiveSettings](../../../Tests/Shared/LiveSettings.cs); never put credentials into committed test commands.

## .NET 11 additions

- `--no-dependencies` skips building referenced projects. Use only when their outputs are current; after client or generator edits, rebuild dependencies.
- `--use-current-runtime` changes restore/build targeting; prefer explicit `-f` for this repository's compatibility matrix.
- `--test-modules` supports exclusion patterns prefixed with `!`. Quote expressions in PowerShell. This mode runs built artifacts; use project evaluation when sources changed.
- `DOTNET_TEST_RUNNER` overrides the runner in the root configuration. Inspect it when behavior unexpectedly resembles VSTest; do not change the repository's runner to work around a local environment setting.

Keep the failing/changed project as the initial scope. Run relevant older-target behavior when exercising a conditional fallback. Use `--coverage` for the installed MTP coverage extension; do not copy VSTest's `--collect` or `--logger trx` options into MTP commands. Confirm optional report extensions before selecting their switches.

Sources reviewed 2026-11-12: [MTP CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-mtp), [runner selection](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test), [.NET 11 SDK additions](https://learn.microsoft.com/cs-cz/dotnet/core/whats-new/dotnet-11/sdk#test-improvements).

---
name: compatibility-net11
description: Adopt .NET 11 APIs or C# 15 features in ManiaAPI.NET while preserving its older target frameworks, source generators, and AOT support. Use when changing framework, language, or dependency requirements.
---

# .NET 11 compatibility

Read the affected project and imported build properties before choosing an API. The libraries currently target .NET 8 through 11 and explicitly select C# 14; samples target .NET 10. Tests inherit their settings from [Tests/Directory.Build.props](../../../Tests/Directory.Build.props). The TMX generator targets .NET Standard 2.0 and uses Roslyn 5.0.0. Proposals in [Docs](../../../Docs) may describe new projects; treat them as designs, not existing code.

## Make the change at the right boundary

- For runtime APIs available only in .NET 11, use `#if NET11_0_OR_GREATER` with an equivalent older-target implementation, or keep the feature in a .NET 11-only application. Keep shared public signatures available on every existing target.
- Check package-provided APIs separately: their availability depends on the referenced version and supported package assets, not just the TFM. A new SDK does not upgrade package references.
- C# 15 is the default for .NET 11 only when the project has not overridden `LangVersion`. Do not introduce `union`, `closed`, or collection-expression arguments into C# 14 sources. Assess compiler, runtime, and serializer requirements before an intentional language upgrade.
- Generator implementation code must remain compatible with its own target and Roslyn dependency. Generated consumer code must respect the consumer's compilation settings. Edit the generator rather than checked-in generated output.
- Preserve `IsTrimmable` and `IsAotCompatible`; retain generated JSON metadata and typed calls instead of adding reflection fallbacks.

Build the affected project across its declared targets and test the changed behavior on .NET 11 and any fallback path. Check `dotnet --info` first; report an unavailable SDK/runtime rather than retargeting to make validation pass. For runtime upgrades, consider the updated CPU baseline when diagnosing startup failures on older machines.

Sources reviewed 2026-11-12: [.NET 11 overview](https://learn.microsoft.com/cs-cz/dotnet/core/whats-new/dotnet-11/overview), [C# 15](https://learn.microsoft.com/cs-cz/dotnet/csharp/whats-new/csharp-15), [runtime requirements](https://learn.microsoft.com/cs-cz/dotnet/core/whats-new/dotnet-11/runtime#hardware-requirements).

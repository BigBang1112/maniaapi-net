---
name: system-text-json-net11
description: Use .NET 11 System.Text.Json metadata, naming, and streaming features in ManiaAPI clients or applications that consume them. Apply when changing JSON contracts or generated serialization, not ordinary model edits unrelated to these features.
---

# System.Text.Json in .NET 11

Start with the existing client's `JsonSerializerContext`, converters, and wire fixtures. The libraries use generated metadata for trimming and AOT. Their contexts are internal, so an application that serializes library models itself needs its own generated context for those types.

## Choose the feature without changing the wire contract

- Use `options.GetTypeInfo<T>()` to obtain `JsonTypeInfo<T>` without a downcast. Use `options.TryGetTypeInfo<T>(out JsonTypeInfo<T>? info)` when missing registration is expected. The latter is not a general exception shield for broken resolvers or configuration.
- Resolve metadata from the existing generated context/options. Do not introduce `DefaultJsonTypeInfoResolver` or `MakeReadOnly(populateMissingResolver: true)` as an AOT workaround. Register missing types in the context.
- Use `JsonNamingPolicy.PascalCase` or `JsonKnownNamingPolicy.PascalCase` only when the upstream schema requires that transformation. Existing PascalCase CLR names may already need no policy. Preserve explicit `JsonPropertyName` values and the Nadeo context's camelCase behavior.
- .NET 11 adds source-generation support for non-public members marked `JsonInclude` and non-public constructors marked `JsonConstructor`. Verify the generated contract with the actual SDK; retain a compatible model shape for older targets.
- For an explicit export of `IAsyncEnumerable<T>`, consider `JsonSerializer.SerializeAsyncEnumerable` with generated `JsonTypeInfo<T>`. `topLevelValues: true` emits JSON Lines; it does not implement any transport protocol's framing. Leave protocol output to the library that owns that protocol.

Gate new APIs for older TFMs unless compatible package assets deliberately provide them. Test representative payloads, missing/null values, and custom converters on both paths. Check that every serialized type is covered by generated metadata before an AOT publish.

Sources reviewed 2026-11-12: [typed metadata](https://learn.microsoft.com/cs-cz/dotnet/standard/serialization/system-text-json/custom-contracts#get-strongly-typed-metadata), [.NET 11 JSON changes](https://learn.microsoft.com/cs-cz/dotnet/core/whats-new/dotnet-11/libraries#systemtextjson-improvements).

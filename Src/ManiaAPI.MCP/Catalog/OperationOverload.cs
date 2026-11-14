using System.Text.Json;
using System.Text.Json.Nodes;

namespace ManiaAPI.MCP.Catalog;

public sealed record OperationOverload(
    JsonObject Schema,
    string Signature,
    JsonNode OutputSchema,
    Func<InvocationContext, JsonElement, CancellationToken, Task<JsonNode?>> Invoke);

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace ManiaAPI.MCP.Catalog;

public sealed record Operation(
    string Id,
    string Toolset,
    string Summary,
    string Requires,
    string? ExchangeClient,
    bool ServiceAccountOnly,
    bool Obsolete,
    OperationOverload[] Overloads)
{
    public bool ReadOnly { get; init; } = true;
    public bool ReturnsBinary { get; init; }
    public bool Idempotent { get; init; }
    public string? PlatformClient { get; init; }
    public bool PlayerOnly { get; init; }
    public string? OAuthProvider { get; init; }
    public string[] OAuthScopes { get; init; } = [];

    public JsonObject Schema(IReadOnlyList<string> sites, bool inferSite)
    {
        var variants = new JsonArray();

        foreach (var overload in Overloads)
        {
            var schema = (JsonObject)overload.Schema.DeepClone();

            if (Overloads.Length > 1)
            {
                CatalogMetadata.PrefixReferences(schema, "#/anyOf/" + variants.Count);
            }

            if (ExchangeClient is not null)
            {
                schema["properties"]!["site"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray(sites.Select(site => (JsonNode?)JsonValue.Create(site)).ToArray())
                };

                if (!inferSite)
                {
                    ((JsonArray)schema["required"]!).Add("site");
                }
            }

            if (PlatformClient is not null)
            {
                schema["properties"]!["platform"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray(Enum.GetNames<MasterServer.TMT.Platform>()
                        .Select(platform => (JsonNode?)JsonValue.Create(platform)).ToArray())
                };

                ((JsonArray)schema["required"]!).Add("platform");
            }

            variants.Add(schema);
        }

        return variants.Count == 1
            ? (JsonObject)variants[0]!.DeepClone()
            : new JsonObject { ["type"] = "object", ["anyOf"] = variants };
    }
}

public sealed record OperationOverload(
    JsonObject Schema,
    string Signature,
    JsonNode OutputSchema,
    Func<InvocationContext, JsonElement, CancellationToken, Task<JsonNode?>> Invoke);

public static class CatalogMetadata
{
    public static JsonObject Input(params (string Name, JsonNode Schema, bool Required)[] parameters)
    {
        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var (name, schema, isRequired) in parameters)
        {
            PrefixReferences(schema, "#/properties/" + name);
            properties[name] = schema;

            if (isRequired)
            {
                required.Add(name);
            }
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }

    public static JsonNode Schema<T>()
    {
        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSchemaExporter.GetJsonSchemaAsNode(metadata,
            new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });
    }

    internal static void PrefixReferences(JsonNode? node, string prefix)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) && value.StartsWith('#'))
            {
                obj["$ref"] = prefix + value[1..];
            }

            foreach (var child in obj.Select(x => x.Value))
            {
                PrefixReferences(child, prefix);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                PrefixReferences(child, prefix);
            }
        }
    }

    public static T Argument<T>(JsonElement arguments, string name, T fallback = default!)
    {
        if (!arguments.TryGetProperty(name, out var value))
        {
            return fallback;
        }

        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSerializer.Deserialize(value, metadata)!;
    }

    public static JsonNode? Result<T>(T value)
    {
        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSerializer.SerializeToNode(value, metadata);
    }
}

using System.Text.Json.Nodes;

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

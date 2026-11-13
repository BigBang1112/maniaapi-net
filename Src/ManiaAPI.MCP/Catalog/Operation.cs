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

    /// <summary>
    /// The public input schema: every overload merged into one flat object schema. Argument combinations
    /// that no single overload accepts are rejected by the engine, which lists the valid signatures.
    /// </summary>
    public JsonObject Schema(IReadOnlyList<string> sites, bool inferSite)
    {
        var schema = CatalogMetadata.Merge(Overloads.Select(overload => overload.Schema));

        if (ExchangeClient is not null)
        {
            schema["properties"]!["site"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Exchange site to query; only the sites this endpoint allows are listed.",
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
                ["description"] = "Trackmania Turbo platform whose master server to query.",
                ["enum"] = new JsonArray(Enum.GetNames<MasterServer.TMT.Platform>()
                    .Select(platform => (JsonNode?)JsonValue.Create(platform)).ToArray())
            };

            ((JsonArray)schema["required"]!).Add("platform");
        }

        return schema;
    }
}

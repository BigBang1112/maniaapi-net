using System.Text.Json;
using System.Text.Json.Nodes;
using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Resources;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;

namespace ManiaAPI.MCP.Catalog;

public static class GbxOperations
{
    public static IEnumerable<Operation> Create()
    {
        foreach (var client in new[] { "tmx", "mx" })
        {
            foreach (var asset in new[] { client == "tmx" ? "track" : "map", "replay" })
            {
                foreach (var suffix in new[] { "gbx_header", "gbx", "gbx_header_node", "gbx_node" })
                {
                    var parameter = asset + "Id";
                    var id = client + "." + asset + "_" + suffix;

                    yield return new Operation(id, "exchange", "Read the Gbx header summary and file link; full object graphs stay outside MCP.",
                        "none", client, false, false,
                    [
                        new OperationOverload(CatalogMetadata.Input((parameter, CatalogMetadata.Schema<long>(), true)),
                            parameter + ": long", OutputSchema(), (context, arguments, token) =>
                                ReadAsync(context, client, asset, arguments.GetProperty(parameter).GetInt64(), token))
                    ]);
                }
            }
        }

        foreach (var update in new[] { false, true })
        {
            var sourceSchema = JsonNode.Parse("""
                {
                  "type": "object",
                  "properties": {
                    "path": {
                      "type": "string"
                    },
                    "url": {
                      "type": "string",
                      "format": "uri"
                    },
                    "resource": {
                      "type": "string"
                    }
                  },
                  "oneOf": [
                    {
                      "required": [
                        "path"
                      ]
                    },
                    {
                      "required": [
                        "url"
                      ]
                    },
                    {
                      "required": [
                        "resource"
                      ]
                    }
                  ],
                  "additionalProperties": false
                }
                """)!;

            var input = CatalogMetadata.Input(
                ("source", sourceSchema, true),
                ("fileName", CatalogMetadata.Schema<string>(), false),
                ("mapId", CatalogMetadata.Schema<Guid>(), false));

            if (!update)
            {
                input["properties"]!.AsObject().Remove("mapId");
            }

            yield return new Operation("nadeo.core." + (update ? "update_map" : "upload_map"), "trackmania2020",
                update ? "Update an uploaded map from a local Map.Gbx path (stdio only), URL, or resource." :
                    "Upload a map from a local Map.Gbx path (stdio only), URL, or resource.",
                "nadeo", null, true, false,
            [
                new OperationOverload(input, "source: {path|url|resource}, fileName?: string" + (update ? ", mapId?: guid" : ""),
                    CatalogMetadata.Schema<NadeoAPI.MapInfo>(), (context, arguments, token) => UploadAsync(context, arguments, update, token))
            ]) { ReadOnly = false };
        }
    }

    private static JsonNode OutputSchema()
    {
        return JsonNode.Parse("""
            {
              "type": "object",
              "properties": {
                "uid": {
                  "type": "string"
                },
                "name": {
                  "type": "string"
                },
                "author": {
                  "type": "string"
                },
                "environment": {
                  "type": "string"
                },
                "medals": {
                  "type": "object"
                },
                "mapUid": {
                  "type": "string"
                },
                "player": {
                  "type": "string"
                },
                "time": {
                  "type": [
                    "integer",
                    "null"
                  ]
                },
                "resourceUri": {
                  "type": "string"
                }
              }
            }
            """)!;
    }

    private static async Task<JsonNode?> ReadAsync(InvocationContext context, string client, string asset, long id, CancellationToken token)
    {
        using var response = client == "tmx"
            ? asset == "track"
                ? await context.Client<TMX.TMX>(client).GetTrackGbxResponseAsync(id, token)
                : await context.Client<TMX.TMX>(client).GetReplayGbxResponseAsync(id, token)
            : asset == "map"
                ? await context.Client<TMX.MX>(client).GetMapGbxResponseAsync(id, token)
                : await context.Client<TMX.MX>(client).GetReplayGbxResponseAsync(id, token);

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        var limit = context.Services.GetRequiredService<McpOptions>().Shaping.MaxResourceBytes;
        using var data = new MemoryStream(await BoundedRead.AllAsync(stream, limit, token));

        var arguments = new JsonObject { [asset + "Id"] = id, ["site"] = context.Site };
        using var document = JsonDocument.Parse(arguments.ToJsonString());
        var uri = ResourceUri.Create(client + "." + asset + "_gbx_response", document.RootElement);

        if (asset == "replay")
        {
            var replay = Gbx.ParseHeaderNode<CGameCtnReplayRecord>(data);

            return new JsonObject
            {
                ["mapUid"] = replay.MapInfo?.Id,
                ["player"] = replay.PlayerLogin,
                ["time"] = replay.Time?.TotalMilliseconds,
                ["resourceUri"] = uri
            };
        }

        var map = Gbx.ParseHeaderNode<CGameCtnChallenge>(data);

        return new JsonObject
        {
            ["uid"] = map.MapUid,
            ["name"] = map.MapName,
            ["author"] = map.AuthorLogin,
            ["environment"] = map.Collection?.ToString(),
            ["medals"] = new JsonObject
            {
                ["author"] = map.AuthorTime?.TotalMilliseconds,
                ["gold"] = map.GoldTime?.TotalMilliseconds,
                ["silver"] = map.SilverTime?.TotalMilliseconds,
                ["bronze"] = map.BronzeTime?.TotalMilliseconds
            },
            ["resourceUri"] = uri
        };
    }

    private static async Task<JsonNode?> UploadAsync(InvocationContext context, JsonElement arguments, bool update, CancellationToken token)
    {
        var source = arguments.GetProperty("source");
        var services = context.Services;
        var limit = services.GetRequiredService<McpOptions>().Shaping.MaxResourceBytes;
        byte[] bytes;
        string? sourceName = null;

        if (source.TryGetProperty("path", out var path))
        {
            if (services.GetRequiredService<IHttpContextAccessor>().HttpContext is not null)
            {
                throw new ArgumentException("Local map paths are available over stdio only.");
            }

            var filename = path.GetString()!;

            if (!filename.EndsWith(".Map.Gbx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The local source must be a .Map.Gbx file.");
            }

            sourceName = Path.GetFileName(filename);
            await using var stream = File.OpenRead(filename);
            bytes = await BoundedRead.AllAsync(stream, limit, token);
        }
        else if (source.TryGetProperty("url", out var url))
        {
            if (!Uri.TryCreate(url.GetString(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") ||
                !string.IsNullOrEmpty(uri.UserInfo))
            {
                throw new ArgumentException("The map URL must use HTTP or HTTPS without embedded credentials.");
            }

            using var http = services.GetRequiredService<IHttpClientFactory>().CreateClient("MapSource");
            using var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            bytes = await BoundedRead.AllAsync(stream, limit, token);
            sourceName = Path.GetFileName(uri.LocalPath);
        }
        else
        {
            var (id, resourceArguments) = ResourceUri.Parse(source.GetProperty("resource").GetString()!);
            var capture = new BinaryCapture();

            await services.GetRequiredService<OperationEngine>().InvokeAsync(id, resourceArguments,
                services.GetRequiredService<Tools.GatewayTools>().Scope(), services,
                cancellationToken: token, capture: capture);

            bytes = capture.Bytes ?? throw new ArgumentException("The resource does not contain a map file.");
        }

        using var data = new MemoryStream(bytes);
        var fileName = arguments.TryGetProperty("fileName", out var supplied)
            ? Path.GetFileName(supplied.GetString()!)
            : string.IsNullOrEmpty(sourceName) ? "Upload.Map.Gbx" : sourceName;

        var client = context.Client<INadeoServices>("nadeo.core");
        var result = update
            ? arguments.TryGetProperty("mapId", out var mapId)
                ? await client.UpdateMapAsync(mapId.GetGuid(), data, fileName, token)
                : await client.UpdateMapAsync(data, fileName, token)
            : await client.UploadMapAsync(data, fileName, token);

        return CatalogMetadata.Result(result);
    }
}

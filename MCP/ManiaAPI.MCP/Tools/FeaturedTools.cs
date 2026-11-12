using System.Text.Json;
using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Tools;

public sealed class FeaturedTools(McpOptions options, OperationEngine engine)
{
    private static readonly (string Name, string Toolset, string Operation, string Description)[] Aliases =
    [
        ("trackmania2020_map", "trackmania2020", "nadeo.live.map_info", "Get official Trackmania 2020 map information and download links."),
        ("trackmania2020_leaderboard", "trackmania2020", "nadeo.live.top_leaderboard", "Get top map records worldwide or in a zone. Use mapUid, and optionally groupUid, zoneId, or paging."),
        ("trackmania2020_campaign", "trackmania2020", "nadeo.live.seasonal_campaigns", "Get seasonal Trackmania 2020 campaigns, playlists, and map identifiers."),
        ("trackmania2020_club", "trackmania2020", "nadeo.live.club", "Get a Trackmania 2020 club's details."),
        ("trackmania2020_matchmaking", "trackmania2020", "nadeo.meet.matchmaking_summary", "Get the current matchmaking summary."),
        ("maniaplanet_title", "maniaplanet", "maniaplanet.web.title_by_uid", "Get a ManiaPlanet title. Trackmania 2/TM2 and ShootMania/SM use ManiaPlanet titles; preserve the exact title UID."),
        ("tmuf_player", "tmuf", "masterserver.tmuf.player_infos", "Get a United Forever player by login, using the United master server."),
        ("tmnf_player", "tmnf", "masterserver.tmnf.player_infos", "Get a Nations Forever player by login, using the Nations master server."),
        ("tmuf_leaderboard", "tmuf", "masterserver.tmuf.ladder_player_rankings", "Get United Forever ladder rankings by league with paging."),
        ("tmnf_leaderboard", "tmnf", "masterserver.tmnf.ladder_player_rankings", "Get Nations Forever ladder rankings by league with paging."),
        ("maniaplanet_player", "maniaplanet", "masterserver.mp4.player_infos", "Get a ManiaPlanet, Trackmania 2/TM2, or ShootMania/SM player by login."),
        ("maniaplanet_leaderboard", "maniaplanet", "masterserver.mp4.campaign_leader_board", "Get a ManiaPlanet campaign leaderboard for an explicit title ID."),
        ("trackmaniaturbo_leaderboard", "trackmaniaturbo", "masterserver.tmt_aggregated.campaign_leader_board_summary", "Get Trackmania Turbo campaign rankings aggregated across PC, XB1, and PS4.")
    ];

    private static readonly Dictionary<string, (string Mode, string Operation)[]> Routes = new(StringComparer.Ordinal)
    {
        ["trackmania2020_leaderboard"] =
        [
            ("top", "nadeo.live.top_leaderboard"),
            ("around", "nadeo.live.surrounding_records"),
            ("position", "nadeo.live.leaderboard_position_by_time")
        ],
        ["trackmania2020_campaign"] =
        [
            ("seasonal", "nadeo.live.seasonal_campaigns"),
            ("weekly_short", "nadeo.live.weekly_short_campaigns"),
            ("weekly_grand", "nadeo.live.weekly_grand_campaigns"),
            ("club", "nadeo.live.club_campaign")
        ],
        ["trackmania2020_club"] =
        [
            ("details", "nadeo.live.club"),
            ("members", "nadeo.live.club_members"),
            ("activities", "nadeo.live.club_activities")
        ],
        ["trackmania2020_matchmaking"] =
        [
            ("summary", "nadeo.meet.matchmaking_summary"),
            ("rankings", "nadeo.meet.matchmaking_rankings"),
            ("player_ranks", "nadeo.meet.matchmaking_player_ranks")
        ],
        ["maniaplanet_title"] =
        [
            ("web", "maniaplanet.web.title_by_uid"),
            ("ingame", "maniaplanet.ingame.title_by_uid"),
            ("download", "maniaplanet.ingame.download_title")
        ],
        ["tmuf_leaderboard"] = [("ladder", "masterserver.tmuf.ladder_player_rankings"), ("campaign", "masterserver.tmuf.campaign_scores")],
        ["tmnf_leaderboard"] = [("ladder", "masterserver.tmnf.ladder_player_rankings"), ("campaign", "masterserver.tmnf.campaign_scores")],
        ["maniaplanet_leaderboard"] = [("campaign", "masterserver.mp4.campaign_leader_board"), ("map", "masterserver.mp4.map_leader_board")],
        ["trackmaniaturbo_leaderboard"] =
        [
            ("campaign", "masterserver.tmt_aggregated.campaign_leader_board_summary"),
            ("map", "masterserver.tmt_aggregated.map_leader_board_summary"),
            ("platform_campaign", "masterserver.tmt.campaign_leader_board_summary"),
            ("platform_map", "masterserver.tmt.map_leader_board_summary")
        ]
    };

    public IEnumerable<Tool> List(GameScope scope)
    {
        var available = engine.Available(scope).ToDictionary(operation => operation.Id, StringComparer.Ordinal);

        foreach (var alias in Aliases)
        {
            if (Enabled(alias.Toolset, alias.Name) && available.TryGetValue(alias.Operation, out var operation))
            {
                if (Routes.TryGetValue(alias.Name, out var routes))
                {
                    var variants = new JsonArray();
                    var outputs = new JsonArray();

                    foreach (var route in routes.Where(route => available.ContainsKey(route.Operation)))
                    {
                        var target = available[route.Operation];
                        var schema = target.Schema(scope.Sites(target), scope.InferSite && scope.Sites(target).Length == 1);
                        var overloads = schema["anyOf"] is JsonArray nested ? nested.ToArray() : [schema];

                        foreach (var overload in overloads)
                        {
                            overload!["properties"]!["mode"] = new JsonObject { ["type"] = "string", ["const"] = route.Mode };

                            if (route.Operation != alias.Operation)
                            {
                                overload["required"]!.AsArray().Add("mode");
                            }
                        }

                        CatalogMetadata.PrefixReferences(schema, "#/anyOf/" + variants.Count);
                        variants.Add(schema);
                        outputs.Add(target.Overloads[0].OutputSchema.DeepClone());
                    }

                    yield return Tool(alias.Name, alias.Description + " Choose mode to select a related view.",
                        new JsonObject { ["type"] = "object", ["anyOf"] = variants }, new JsonObject { ["anyOf"] = outputs });
                }
                else
                {
                    yield return Tool(alias.Name, alias.Description,
                        operation.Schema(scope.Sites(operation), scope.InferSite && scope.Sites(operation).Length == 1), operation.Overloads[0].OutputSchema);
                }
            }
        }

        if (Enabled("trackmania2020", "trackmania2020_player") && available.ContainsKey("nadeo.core.player_zone"))
        {
            yield return Tool("trackmania2020_player", "Resolve an account ID or display name, then get the player's zone, trophies, and club tag when available.",
                Parse("""
                    {
                      "type": "object",
                      "properties": {
                        "accountId": {
                          "type": "string",
                          "format": "uuid"
                        },
                        "displayName": {
                          "type": "string"
                        }
                      },
                      "oneOf": [
                        {
                          "required": [
                            "accountId"
                          ]
                        },
                        {
                          "required": [
                            "displayName"
                          ]
                        }
                      ],
                      "additionalProperties": false
                    }
                    """), Parse("""
                        {
                          "type": "object",
                          "properties": {
                            "accountId": {
                              "type": "string"
                            },
                            "zone": {},
                            "trophies": {},
                            "clubTags": {}
                          }
                        }
                        """));
        }

        if (Enabled("trackmania2020", "trackmania2020_track_of_the_day") && available.ContainsKey("nadeo.live.track_of_the_days"))
        {
            yield return Tool("trackmania2020_track_of_the_day", "Get Track of the Day rotations and the current Cup of the Day when available.",
                Parse("""
                    {
                      "type": "object",
                      "properties": {
                        "length": {
                          "type": "integer",
                          "minimum": 1,
                          "default": 1
                        },
                        "offset": {
                          "type": "integer",
                          "minimum": 0,
                          "default": 0
                        }
                      },
                      "additionalProperties": false
                    }
                    """),
                Parse("""
                    {
                      "type": "object",
                      "properties": {
                        "trackOfTheDay": {},
                        "cupOfTheDay": {}
                      }
                    }
                    """));
        }

        var searchOperations = new[] { "tmx.search_tracks", "mx.search_maps" }
            .Where(available.ContainsKey).Select(id => available[id]).ToArray();

        if (searchOperations.Length == 0)
        {
            yield break;
        }

        var sites = string.Join(", ", scope.TmxSites.Concat(scope.MxSites));

        if (Enabled("exchange", "exchange_search"))
        {
            var input = Parse("""
                {
                  "type": "object",
                  "properties": {
                    "site": {
                      "type": "string"
                    },
                    "parameters": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string"
                        },
                        "author": {
                          "type": "string"
                        },
                        "authorUserId": {
                          "type": "integer"
                        },
                        "count": {
                          "type": "integer",
                          "minimum": 1,
                          "maximum": 100
                        },
                        "after": {
                          "type": "integer"
                        },
                        "before": {
                          "type": "integer"
                        },
                        "id": {
                          "type": "array",
                          "items": {
                            "type": "integer"
                          },
                          "maxItems": 100
                        }
                      },
                      "additionalProperties": false
                    }
                  },
                  "required": [
                    "parameters"
                  ],
                  "additionalProperties": false
                }
                """).AsObject();

            input["properties"]!["site"]!["enum"] = new JsonArray(scope.TmxSites.Concat(scope.MxSites)
                .Select(site => (JsonNode?)JsonValue.Create(site)).ToArray());

            if (!scope.InferSite || scope.TmxSites.Length + scope.MxSites.Length != 1)
            {
                input["required"]!.AsArray().Add("site");
            }

            yield return Tool("exchange_search", "Find community maps or tracks. Allowed sites: " + sites + ". TM2 uses Maniaplanet; ShootMania uses Shootmania.",
                input, new JsonObject());
        }

        if (Enabled("exchange", "exchange_item"))
        {
            var input = Parse("""
                {
                  "type": "object",
                  "properties": {
                    "id": {
                      "type": "integer",
                      "minimum": 1
                    },
                    "kind": {
                      "type": "string",
                      "enum": [
                        "map",
                        "mappack"
                      ],
                      "default": "map"
                    },
                    "site": {
                      "type": "string"
                    },
                    "gbxHeader": {
                      "type": "boolean",
                      "default": false
                    }
                  },
                  "required": [
                    "id"
                  ],
                  "additionalProperties": false
                }
                """).AsObject();

            input["properties"]!["site"]!["enum"] = new JsonArray(scope.TmxSites.Concat(scope.MxSites)
                .Select(site => (JsonNode?)JsonValue.Create(site)).ToArray());

            if (!scope.InferSite || scope.TmxSites.Length + scope.MxSites.Length != 1)
            {
                input["required"]!.AsArray().Add("site");
            }

            yield return Tool("exchange_item", "Get community map or mappack details, authors, and file links. Allowed sites: " + sites + ".",
                input, Parse("""
                    {
                      "type": "object",
                      "properties": {
                        "details": {},
                        "file": {},
                        "header": {}
                      }
                    }
                    """));
        }
    }

    public async Task<JsonObject> InvokeAsync(string name, JsonElement arguments, GameScope scope,
        IServiceProvider services, CancellationToken cancellationToken)
    {
        var alias = Aliases.FirstOrDefault(alias => alias.Name == name);

        if (alias.Name is not null)
        {
            var operation = alias.Operation;

            if (arguments.TryGetProperty("mode", out var mode))
            {
                operation = Routes[alias.Name].First(route => route.Mode == mode.GetString()).Operation;
            }

            var input = JsonNode.Parse(arguments.GetRawText())!.AsObject();
            input.Remove("mode");

            return await Call(operation, input, scope, services, cancellationToken);
        }

        if (name == "exchange_search")
        {
            var site = Site(arguments, scope);
            var operation = scope.TmxSites.Contains(site) ? "tmx.search_tracks" : "mx.search_maps";
            var input = JsonNode.Parse(arguments.GetRawText())!.AsObject();
            input["site"] = site;

            return await Call(operation, input, scope, services, cancellationToken);
        }

        if (name == "exchange_item")
        {
            return await ExchangeItem(arguments, scope, services, cancellationToken);
        }

        var data = new JsonObject();
        var notes = new JsonArray();
        var available = engine.Available(scope).Select(operation => operation.Id).ToHashSet(StringComparer.Ordinal);

        if (name == "trackmania2020_player")
        {
            string account;

            if (arguments.TryGetProperty("accountId", out var supplied))
            {
                account = supplied.GetString()!;
            }
            else
            {
                var displayName = arguments.GetProperty("displayName").GetString()!;
                var resolved = await Call("trackmania.api.account_ids", new JsonObject
                {
                    ["displayNames"] = new JsonArray(displayName)
                }, scope, services, cancellationToken);

                account = resolved["data"]?[displayName]?.GetValue<string>()
                    ?? throw new ArgumentException("The display name was not found. Supply accountId if available.");
            }

            data["accountId"] = account;
            await Add("zone", "nadeo.core.player_zone", new JsonObject { ["accountId"] = account });

            if (available.Contains("nadeo.core.player_trophy_summary"))
            {
                await Add("trophies", "nadeo.core.player_trophy_summary", new JsonObject { ["accountId"] = account });
            }

            if (available.Contains("nadeo.core.player_club_tags"))
            {
                await Add("clubTags", "nadeo.core.player_club_tags", new JsonObject { ["accountIds"] = new JsonArray(account) });
            }
        }
        else if (name == "trackmania2020_track_of_the_day")
        {
            await Add("trackOfTheDay", "nadeo.live.track_of_the_days", new JsonObject
            {
                ["length"] = arguments.TryGetProperty("length", out var length) ? length.GetInt32() : 1,
                ["offset"] = arguments.TryGetProperty("offset", out var offset) ? offset.GetInt32() : 0
            });

            if (available.Contains("nadeo.meet.current_cup_of_the_day"))
            {
                await Add("cupOfTheDay", "nadeo.meet.current_cup_of_the_day", new JsonObject());
            }
        }
        else
        {
            throw new ArgumentException("Unknown featured tool.");
        }

        var shaped = ResultShaper.Shape(data, null, options.Shaping);
        foreach (var note in notes)
        {
            shaped["truncation"]!.AsArray().Add(note?.DeepClone());
        }

        if (shaped.ToJsonString().Length > options.Shaping.MaxChars)
        {
            shaped["data"] = null;
            shaped["truncation"] = new JsonArray("The combined result exceeded MaxChars; use a narrower catalog operation.");
        }

        return shaped;

        async Task Add(string field, string operation, JsonObject input)
        {
            var result = await Call(operation, input, scope, services, cancellationToken);
            data[field] = result["data"]?.DeepClone();

            foreach (var note in result["truncation"]!.AsArray())
            {
                notes.Add(note?.DeepClone());
            }
        }
    }

    private async Task<JsonObject> ExchangeItem(JsonElement arguments, GameScope scope, IServiceProvider services, CancellationToken token)
    {
        var site = Site(arguments, scope);
        var prefix = scope.TmxSites.Contains(site) ? "tmx" : "mx";
        var kind = arguments.TryGetProperty("kind", out var supplied) ? supplied.GetString() : "map";
        var id = arguments.GetProperty("id").GetInt64();
        var search = prefix + "." + (kind switch
        {
            "map" => prefix == "tmx" ? "search_tracks" : "search_maps",
            "mappack" => prefix == "tmx" ? "search_trackpacks" : "search_mappacks",
            _ => prefix == "tmx" ? "replays" : "search_replays"
        });

        var details = await Call(search, new JsonObject
        {
            ["site"] = site,
            ["parameters"] = new JsonObject { ["id"] = new JsonArray(JsonValue.Create(id)), ["count"] = 1 }
        }, scope, services, token);

        var data = new JsonObject { ["details"] = details["data"]?.DeepClone() };

        if (kind != "mappack")
        {
            var asset = prefix == "tmx" ? "track" : "map";
            var input = new JsonObject { ["site"] = site, [asset + "Id"] = id };
            var file = await Call(prefix + "." + asset + "_gbx_url", input, scope, services, token);
            using var inputDocument = JsonDocument.Parse(input.ToJsonString());
            data["file"] = new JsonObject
            {
                ["url"] = file["data"]?.DeepClone(),
                ["resourceUri"] = Resources.ResourceUri.Create(prefix + "." + asset + "_gbx_response", inputDocument.RootElement)
            };

            if (arguments.TryGetProperty("gbxHeader", out var header) && header.GetBoolean())
            {
                var summary = await Call(prefix + "." + asset + "_gbx_header", input, scope, services, token);
                data["header"] = summary["data"]?.DeepClone();
            }
        }

        return ResultShaper.Shape(data, null, options.Shaping);
    }

    private Task<JsonObject> Call(string operation, JsonObject input, GameScope scope, IServiceProvider services, CancellationToken token)
    {
        using var document = JsonDocument.Parse(input.ToJsonString());

        return engine.InvokeAsync(operation, document.RootElement.Clone(), scope, services, cancellationToken: token);
    }

    private static string Site(JsonElement arguments, GameScope scope)
    {
        var available = scope.TmxSites.Concat(scope.MxSites).ToArray();
        var site = arguments.TryGetProperty("site", out var value) ? value.GetString() : null;

        if (site is null && scope.InferSite && available.Length == 1)
        {
            return available[0];
        }

        return site is not null && available.Contains(site) ? site :
            throw new ArgumentException("Choose a configured exchange site in this endpoint's scope.");
    }

    private bool Enabled(string toolset, string name)
    {
        return !options.Featured.TryGetValue(toolset, out var configured) || configured.Tools is null || configured.Tools.Contains(name);
    }

    private static Tool Tool(string name, string description, JsonNode input, JsonNode output)
    {
        using var inputDocument = JsonDocument.Parse(input.ToJsonString());
        var resultSchema = CompactSchema(output, 0);
        var envelope = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["data"] = new JsonObject { ["anyOf"] = new JsonArray(resultSchema, new JsonObject { ["type"] = "null" }) },
                ["truncation"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string" } }
            },
            ["required"] = new JsonArray("data", "truncation")
        };
        using var outputDocument = JsonDocument.Parse(envelope.ToJsonString());

        return new Tool
        {
            Name = name,
            Description = description,
            InputSchema = inputDocument.RootElement.Clone(),
            OutputSchema = outputDocument.RootElement.Clone(),
            Annotations = new ToolAnnotations { ReadOnlyHint = true, DestructiveHint = false, OpenWorldHint = true, IdempotentHint = true }
        };
    }

    private static JsonNode Parse(string value) => JsonNode.Parse(value)!;

    private static JsonNode CompactSchema(JsonNode schema, int depth)
    {
        if (schema is not JsonObject obj)
        {
            return schema.DeepClone();
        }

        var result = new JsonObject();

        foreach (var key in new[] { "type", "format", "enum" })
        {
            if (obj[key] is { } value)
            {
                result[key] = value.DeepClone();
            }
        }

        if (obj["anyOf"] is JsonArray variants)
        {
            var unique = variants.Select(variant => CompactSchema(variant!, depth))
                .GroupBy(variant => variant.ToJsonString(), StringComparer.Ordinal).Select(group => group.First()).ToArray();

            result["anyOf"] = new JsonArray(unique);
        }

        if (obj["properties"] is JsonObject properties && depth >= 0)
        {
            result["properties"] = new JsonObject(properties.Take(8).Select(property =>
                new KeyValuePair<string, JsonNode?>(property.Key, CompactSchema(property.Value!, depth - 1))));
        }

        if (obj["items"] is { } items && depth >= 0)
        {
            result["items"] = CompactSchema(items, depth - 1);
        }

        return result;
    }
}

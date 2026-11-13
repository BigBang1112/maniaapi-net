using System.Text.Json;
using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ManiaAPI.MCP.Tools;

public sealed class GatewayTools(
    McpOptions options,
    OperationEngine engine,
    OperationSearch search,
    IHttpContextAccessor httpContext,
    ILogger<GatewayTools> logger,
    WriteConfirmation confirmation,
    FeaturedTools featured)
{
    private static readonly JsonElement SearchSchema = ParseSchema("""
        {
          "type": "object",
          "properties": {
            "query": {
              "type": "string",
              "maxLength": 1000
            },
            "game": {
              "type": "string"
            },
            "toolset": {
              "type": "string",
              "enum": [
                "trackmania2020",
                "exchange",
                "tmuf",
                "tmnf",
                "maniaplanet",
                "trackmaniaturbo"
              ]
            },
            "readOnly": {
              "type": "boolean"
            },
            "requires": {
              "type": "string",
              "enum": [
                "none",
                "nadeo",
                "oauth"
              ]
            },
            "limit": {
              "type": "integer",
              "minimum": 1,
              "maximum": 50,
              "default": 8
            }
          },
          "required": [
            "query"
          ],
          "additionalProperties": false
        }
        """);

    private static readonly JsonElement DescribeSchema = ParseSchema("""
        {
          "type": "object",
          "properties": {
            "operations": {
              "type": "array",
              "items": {
                "type": "string"
              },
              "minItems": 1,
              "maxItems": 10
            }
          },
          "required": [
            "operations"
          ],
          "additionalProperties": false
        }
        """);

    private static readonly JsonElement InvokeSchema = ParseSchema("""
        {
          "type": "object",
          "properties": {
            "operation": {
              "type": "string"
            },
            "arguments": {
              "type": "object"
            },
            "confirm": {
              "type": "boolean",
              "default": false
            },
            "shape": {
              "type": "object",
              "properties": {
                "fields": {
                  "type": "array",
                  "items": {
                    "type": "string"
                  },
                  "maxItems": 100
                },
                "maxItems": {
                  "type": "integer",
                  "minimum": 1,
                  "maximum": 10000
                },
                "maxChars": {
                  "type": "integer",
                  "minimum": 256
                }
              },
              "additionalProperties": false
            }
          },
          "required": [
            "operation",
            "arguments"
          ],
          "additionalProperties": false
        }
        """);

    public GameScope Scope()
    {
        var path = httpContext.HttpContext?.Request.Path.Value;
        var endpoint = path?.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault();

        return GameScope.Create(options, endpoint);
    }

    public ListToolsResult List()
    {
        var scope = Scope();
        var sites = string.Join(", ", scope.TmxSites.Concat(scope.MxSites));

        List<Tool> tools =
        [
            CreateTool("maniaapi_search",
                "Find ManiaAPI operations by keywords. Game accepts TM2/Trackmania 2/TrackMania² and ShootMania/SM. " +
                "Trackmania 2 uses ManiaPlanet APIs and MX Maniaplanet; ShootMania uses MX Shootmania. " +
                "Explicit titles/environments remain in the query. Available exchange sites: " + sites + "." +
                MissingCredentialsNote(scope),
                SearchSchema, true),
            CreateTool("maniaapi_describe", "Describe up to 10 operation IDs, their schemas, overloads, authentication and pagination.",
                DescribeSchema, true),
            CreateTool("maniaapi_invoke", "Invoke one catalog operation within this endpoint's scope. Writes require confirmation. Credentials remain on the server.",
                InvokeSchema, false)
        ];

        tools.AddRange(featured.List(scope));

        if (options.ExposeOperationsAsTools)
        {
            foreach (var operation in engine.Available(scope))
            {
                tools.Add(CreateTool(operation.Id.Replace('.', '_'), operation.Summary,
                    OperationToolSchema(operation, scope), operation.ReadOnly));
            }
        }

        return new ListToolsResult
        {
            Tools = tools,
            TimeToLive = TimeSpan.FromHours(24),
            CacheScope = CacheScope.Public
        };
    }

    public async ValueTask<CallToolResult> CallAsync(
        CallToolRequestParams request,
        IServiceProvider services,
        CancellationToken cancellationToken,
        McpServer? server = null)
    {
        try
        {
            var scope = Scope();
            var args = request.Arguments ?? new Dictionary<string, JsonElement>();
            var tool = List().Tools.FirstOrDefault(x => x.Name == request.Name);

            if (tool is null)
            {
                throw new ArgumentException("Unknown tool in this endpoint's scope.");
            }

            var input = new JsonObject(args.Select(x => new KeyValuePair<string, JsonNode?>(x.Key,
                JsonNode.Parse(x.Value.GetRawText()))));

            engine.ValidateArguments(tool.InputSchema, Parse(input.ToJsonString()));

            JsonObject result;

            switch (request.Name)
            {
                case "maniaapi_search":
                    result = Search(args, scope);
                    break;

                case "maniaapi_describe":
                    result = Describe(args, scope);
                    break;

                case "maniaapi_invoke":
                    var shape = args.TryGetValue("shape", out var shapeValue)
                        ? new ResultShape(
                            shapeValue.TryGetProperty("fields", out var fields) ? fields.EnumerateArray().Select(x => x.GetString()!).ToArray() : null,
                            shapeValue.TryGetProperty("maxItems", out var items) ? items.GetInt32() : null,
                            shapeValue.TryGetProperty("maxChars", out var chars) ? chars.GetInt32() : null)
                        : null;

                    var id = RequiredString(args, "operation");
                    var invokeArguments = Required(args, "arguments");
                    var descriptor = engine.Describe(id, scope);
                    engine.ValidateArguments(Parse(descriptor.Schema(scope.Sites(descriptor),
                        scope.InferSite && scope.Sites(descriptor).Length == 1).ToJsonString()), invokeArguments);

                    var approved = Confirm(descriptor, invokeArguments, scope, args, request, server);

                    result = await engine.InvokeAsync(id, invokeArguments,
                        scope, services, shape, cancellationToken, confirmed: approved);
                    break;

                default:
                    if (featured.List(scope).Any(tool => tool.Name == request.Name))
                    {
                        result = await featured.InvokeAsync(request.Name, Parse(input.ToJsonString()), scope, services, cancellationToken);
                        break;
                    }

                    var operation = options.ExposeOperationsAsTools
                        ? engine.Available(scope).FirstOrDefault(x => x.Id.Replace('.', '_') == request.Name)
                        : null;

                    if (operation is null)
                    {
                        throw new ArgumentException("Unknown tool in this endpoint's scope.");
                    }

                    var arguments = new JsonObject(args.Select(x => new KeyValuePair<string, JsonNode?>(x.Key,
                        JsonNode.Parse(x.Value.GetRawText()))));
                    arguments.Remove("confirm");

                    var directArguments = Parse(arguments.ToJsonString());
                    var directApproved = Confirm(operation, directArguments, scope, args, request, server);

                    result = await engine.InvokeAsync(operation.Id, directArguments, scope,
                        services, cancellationToken: cancellationToken, confirmed: directApproved);
                    break;
            }

            logger.LogInformation("MCP tool {Tool} completed", request.Name);

            return ToolResult(result);
        }
        catch (InputRequiredException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Upstream exception messages and request arguments can contain secrets. Log only the
            // exception type and the tool name; return a sanitized, actionable error to the client.
            logger.LogWarning("MCP tool {Tool} failed ({ExceptionType})", request.Name, exception.GetType().Name);

            return ToolResult(OperationEngine.Error(exception), true);
        }
    }

    private JsonObject Search(IDictionary<string, JsonElement> args, GameScope scope)
    {
        var query = RequiredString(args, "query");
        var game = OptionalString(args, "game");

        if (game is not null)
        {
            scope = scope.ForGame(game);
        }

        var toolset = OptionalString(args, "toolset");
        var requires = OptionalString(args, "requires");

        if (toolset is not null && !McpOptions.SupportedToolsets.Contains(toolset))
        {
            throw new ArgumentException("toolset must use a supported game or exchange toolset name.");
        }

        if (requires is not null && requires is not ("none" or "nadeo" or "oauth"))
        {
            throw new ArgumentException("requires must be none, nadeo, or oauth.");
        }

        bool Matches(Operation operation) =>
            (toolset is null || operation.Toolset == toolset) &&
            (requires is null || operation.Requires == requires) &&
            (!args.TryGetValue("readOnly", out var readOnly) || operation.ReadOnly == readOnly.GetBoolean());

        var limit = args.TryGetValue("limit", out var limitValue) ? limitValue.GetInt32() : 8;
        var matches = new JsonArray();

        foreach (var operation in search.Search(engine.Available(scope).Where(Matches), query, limit))
        {
            var site = operation.ExchangeClient is null ? "" : "site: " + string.Join("|", scope.Sites(operation)) + "; ";
            var platform = operation.PlatformClient is null ? "" : "platform: " + string.Join("|", Enum.GetNames<MasterServer.TMT.Platform>()) + "; ";
            var signature = operation.Overloads[0].Signature;

            matches.Add($"{operation.Id}({site}{platform}{signature}) — {operation.Summary} [{(operation.ReadOnly ? "read" : "write")}{(operation.Obsolete ? ", obsolete" : "")}]");
        }

        var result = new JsonObject
        {
            ["query"] = query,
            ["game"] = game,
            ["operations"] = matches
        };

        // Enabled operations that lack only credentials are still reported, so the assistant can tell the user
        // what to configure instead of concluding the API doesn't exist.
        var missing = engine.MissingCredentials(scope).Where(x => Matches(x.Operation)).ToDictionary(x => x.Operation, x => x.Credentials);
        var unavailable = new JsonArray();

        foreach (var operation in search.Search(missing.Keys, query, Math.Min(limit, 5)))
        {
            unavailable.Add($"{operation.Id} — {operation.Summary} [needs {missing[operation]}]");
        }

        if (unavailable.Count > 0)
        {
            result["missingCredentials"] = unavailable;
            result["missingCredentialsNote"] = "These operations exist but this server has no credentials for them. " +
                "Tell the user which settings to add rather than assuming the data can't be fetched.";
        }

        return result;
    }

    private string MissingCredentialsNote(GameScope scope)
    {
        var available = engine.Available(scope).Select(x => x.Toolset).ToHashSet(StringComparer.Ordinal);
        var unconfigured = engine.MissingCredentials(scope)
            .Where(x => !available.Contains(x.Operation.Toolset))
            .GroupBy(x => x.Operation.Toolset, StringComparer.Ordinal)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key} (needs {string.Join(" or ", x.Select(y => y.Credentials).Distinct().Order(StringComparer.Ordinal))})")
            .ToArray();

        return unconfigured.Length == 0
            ? ""
            : " Toolsets enabled without credentials on this server: " + string.Join("; ", unconfigured) + ".";
    }

    private JsonObject Describe(IDictionary<string, JsonElement> args, GameScope scope)
    {
        var ids = Required(args, "operations").EnumerateArray().Select(x => x.GetString()!).ToArray();

        if (ids.Length is < 1 or > 10)
        {
            throw new ArgumentException("Supply between 1 and 10 operation IDs.");
        }

        var descriptions = new JsonArray();

        foreach (var id in ids)
        {
            var operation = engine.Describe(id, scope);
            var schema = operation.Schema(scope.Sites(operation), scope.InferSite && scope.Sites(operation).Length == 1);
            var related = search.Related(operation, engine.Available(scope), 5)
                .Select(x => (JsonNode?)JsonValue.Create(x.Id))
                .ToArray();

            descriptions.Add(new JsonObject
            {
                ["id"] = id,
                ["summary"] = operation.Summary,
                ["toolset"] = operation.Toolset,
                ["supportedGames"] = new JsonArray((operation.ExchangeClient is null
                    ? new[] { operation.Toolset }
                    : scope.Sites(operation).Select(site => site switch
                    {
                        "Trackmania" => "trackmania2020",
                        "Maniaplanet" => "Trackmania 2 / TM2",
                        "Shootmania" => "ShootMania / SM",
                        _ => site
                    }).ToArray()).Select(game => (JsonNode?)JsonValue.Create(game)).ToArray()),
                ["inputSchema"] = schema,
                ["resultOutline"] = Outline(operation.Overloads[0].OutputSchema, 2),
                ["signatures"] = new JsonArray(operation.Overloads.Select(x => (JsonNode?)JsonValue.Create(x.Signature)).ToArray()),
                ["requires"] = operation.Requires,
                ["serviceAccountOnly"] = operation.Requires == "nadeo" ? operation.ServiceAccountOnly : null,
                ["oauth"] = operation.OAuthProvider is not null ? new JsonObject
                {
                    ["provider"] = operation.OAuthProvider,
                    ["grantType"] = operation.PlayerOnly ? "authorization_code" : "client_credentials",
                    ["optional"] = operation.Requires == "none",
                    ["scopes"] = new JsonArray(operation.OAuthScopes.Select(scope => (JsonNode?)JsonValue.Create(scope)).ToArray())
                } : null,
                ["pagination"] = new JsonArray(new[] { "length", "offset", "count", "after", "before" }
                    .Where(name => operation.Overloads.Any(x => x.Signature.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => (JsonNode?)JsonValue.Create(x)).ToArray()),
                ["allowedSites"] = new JsonArray((operation.ExchangeClient is null ? [] : scope.Sites(operation))
                    .Select(x => (JsonNode?)JsonValue.Create(x)).ToArray()),
                ["relatedOperations"] = new JsonArray(related),
                ["whenToUse"] = OperationOverrides.WhenToUse(id),
                ["example"] = new JsonObject
                {
                    ["operation"] = id,
                    ["arguments"] = Example(schema, 0)
                },
                ["readOnly"] = operation.ReadOnly
            });
        }

        return new JsonObject { ["operations"] = descriptions };
    }

    private static JsonNode? Example(JsonNode schema, int depth)
    {
        if (depth > 5)
        {
            return null;
        }

        if (schema["anyOf"] is JsonArray variants && variants.Count > 0)
        {
            return Example(variants[0]!, depth + 1);
        }

        if (schema["enum"] is JsonArray values && values.Count > 0)
        {
            return values[0]?.DeepClone();
        }

        if (schema["properties"] is JsonObject properties)
        {
            var result = new JsonObject();

            foreach (var field in schema["required"] as JsonArray ?? new JsonArray())
            {
                var name = field!.GetValue<string>();
                result[name] = Example(properties[name]!, depth + 1);
            }

            return result;
        }

        var type = schema["type"] is JsonValue value ? value.GetValue<string>() : null;

        return type switch
        {
            "array" => new JsonArray(),
            "integer" or "number" => JsonValue.Create(1),
            "boolean" => JsonValue.Create(false),
            "string" when schema["format"]?.ToString() == "uuid" => JsonValue.Create("00000000-0000-0000-0000-000000000001"),
            "string" => JsonValue.Create("example"),
            _ => null
        };
    }

    private static JsonNode? Outline(JsonNode? schema, int depth)
    {
        if (schema is not JsonObject obj)
        {
            return schema?.DeepClone();
        }

        var result = new JsonObject();

        foreach (var (name, value) in obj)
        {
            if (name == "properties" && value is JsonObject properties && depth > 0)
            {
                result[name] = new JsonObject(properties.Select(x => new KeyValuePair<string, JsonNode?>(x.Key,
                    Outline(x.Value, depth - 1))));
            }
            else if (name is "type" or "format" or "enum" or "$ref")
            {
                result[name] = value?.DeepClone();
            }
            else if (name == "items" && depth > 0)
            {
                result[name] = Outline(value, depth - 1);
            }
        }

        return result;
    }

    private Tool CreateTool(string name, string description, JsonElement schema, bool readOnly)
    {
        return new Tool
        {
            Name = name,
            Description = description,
            InputSchema = schema,
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = readOnly,
                DestructiveHint = options.AllowWrites && !readOnly,
                OpenWorldHint = true
            }
        };
    }

    private bool Confirm(Operation operation, JsonElement arguments, GameScope scope,
        IDictionary<string, JsonElement> args, CallToolRequestParams request, McpServer? server)
    {
        var caller = httpContext.HttpContext?.Items["ManiaApiIdentity"]?.ToString() ?? "stdio";
        var scopeKey = string.Join(',', scope.Toolsets.Order(StringComparer.Ordinal)) + ":" +
            string.Join(',', scope.TmxSites.Concat(scope.MxSites));

        return confirmation.Authorize(operation, arguments, scopeKey, caller,
            args.TryGetValue("confirm", out var value) && value.ValueKind == JsonValueKind.True, request, server);
    }

    private static JsonElement OperationToolSchema(Operation operation, GameScope scope)
    {
        var schema = operation.Schema(scope.Sites(operation), scope.InferSite && scope.Sites(operation).Length == 1);
        schema["properties"]!["confirm"] = new JsonObject { ["type"] = "boolean", ["default"] = false };

        return Parse(schema.ToJsonString());
    }

    private static CallToolResult ToolResult(JsonObject value, bool error = false)
    {
        var json = value.ToJsonString();

        List<ContentBlock> content = [new TextContentBlock { Text = json }];

        foreach (var uri in ResourceLinks(value).Distinct(StringComparer.Ordinal))
        {
            content.Add(new ResourceLinkBlock { Uri = uri, Name = "Download resource" });
        }

        return new CallToolResult
        {
            Content = content,
            StructuredContent = Parse(json),
            IsError = error
        };
    }

    private static IEnumerable<string> ResourceLinks(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["resourceUri"] is JsonValue value && value.TryGetValue<string>(out var uri) && uri.StartsWith("maniaapi://", StringComparison.Ordinal))
            {
                yield return uri;
            }

            foreach (var child in obj.Select(property => property.Value))
            {
                foreach (var link in ResourceLinks(child))
                {
                    yield return link;
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                foreach (var link in ResourceLinks(child))
                {
                    yield return link;
                }
            }
        }
    }

    private static JsonElement Required(IDictionary<string, JsonElement> arguments, string name)
    {
        return arguments.TryGetValue(name, out var value)
            ? value
            : throw new ArgumentException($"Missing required field: {name}.");
    }

    private static string RequiredString(IDictionary<string, JsonElement> arguments, string name)
    {
        return Required(arguments, name).GetString() ?? throw new ArgumentException($"{name} cannot be null.");
    }

    private static string? OptionalString(IDictionary<string, JsonElement> arguments, string name)
    {
        return arguments.TryGetValue(name, out var value) ? value.GetString() : null;
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }

    private static JsonElement ParseSchema(string json)
    {
        return Parse(JsonNode.Parse(json)!.ToJsonString());
    }
}

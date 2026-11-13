using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Resources;
using ManiaAPI.NadeoAPI;
using ManiaAPI.TrackmaniaAPI;

namespace ManiaAPI.MCP.Engine;

public sealed class OperationEngine
{
    public static readonly ActivitySource Activities = new("ManiaAPI.MCP");
    public static readonly Meter Metrics = new("ManiaAPI.MCP");

    private static readonly Histogram<double> Duration = Metrics.CreateHistogram<double>("maniaapi.operation.duration", "ms");
    private static readonly Histogram<long> ResultSize = Metrics.CreateHistogram<long>("maniaapi.operation.result.size", "characters");

    private readonly McpOptions options;
    private readonly UpstreamHealth health;
    private readonly HashSet<string> sensitiveValues;
    private readonly IReadOnlyDictionary<string, Operation> operations;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> audienceLimits = new();
    private readonly ConcurrentDictionary<string, JsonSchema> schemas = new();

    public OperationEngine(McpOptions options, IEnumerable<Operation> operations, UpstreamHealth? health = null)
    {
        this.options = options;
        this.health = health ?? new UpstreamHealth(options);
        this.operations = operations.ToDictionary(x => x.Id, StringComparer.Ordinal);

        sensitiveValues = new[]
            {
                options.NadeoAPI.Login, options.NadeoAPI.Password,
                options.TrackmaniaAPI.ClientId, options.TrackmaniaAPI.ClientSecret,
                options.ManiaPlanet.ClientId, options.ManiaPlanet.ClientSecret,
                options.TrackmaniaWS.Username, options.TrackmaniaWS.Password
            }
            .Concat(options.Http.ApiKeys)
            .Where(value => !string.IsNullOrEmpty(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.Ordinal);
    }

    public IEnumerable<Operation> Available(GameScope scope)
    {
        return operations.Values.Where(operation => IsEnabled(operation, scope) && MissingCredentials(operation) is null);
    }

    /// <summary>
    /// Enabled operations in scope that are hidden only because their credentials aren't configured.
    /// Operations that are switched off (toolsets, master server games, writes, account linking) aren't listed.
    /// </summary>
    public IEnumerable<(Operation Operation, string Credentials)> MissingCredentials(GameScope scope)
    {
        foreach (var operation in operations.Values)
        {
            if (IsEnabled(operation, scope) && MissingCredentials(operation) is { } credentials)
            {
                yield return (operation, credentials);
            }
        }
    }

    public Operation Describe(string id, GameScope scope)
    {
        if (!operations.TryGetValue(id, out var operation) || !IsEnabled(operation, scope))
        {
            throw new ArgumentException("Unknown operation in this endpoint's scope. Use maniaapi_search.");
        }

        if (MissingCredentials(operation) is { } credentials)
        {
            throw new ArgumentException($"This operation needs {credentials}, which this server doesn't have.");
        }

        return operation;
    }

    public async Task<JsonObject> InvokeAsync(
        string id,
        JsonElement arguments,
        GameScope scope,
        IServiceProvider services,
        ResultShape? shape = null,
        CancellationToken cancellationToken = default,
        bool confirmed = false,
        BinaryCapture? capture = null)
    {
        var operation = Describe(id, scope);

        if (!operation.ReadOnly && !confirmed)
        {
            throw new ArgumentException("This write requires explicit confirmation.");
        }

        if (!operation.ReadOnly && services.GetRequiredService<IHttpContextAccessor>().HttpContext is not null &&
            !services.GetRequiredService<WriteHostingGuard>().Validated)
        {
            throw new ArgumentException("HTTP writes are unavailable until all listening addresses have been verified as loopback.");
        }

        if (capture is not null && (!operation.ReadOnly || !operation.ReturnsBinary))
        {
            throw new ArgumentException("Only read-only binary operations can be read as resources.");
        }

        ResultShaper.Shape(null, shape, options.Shaping);

        if (arguments.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("arguments must be a JSON object.");
        }

        string? site = null;

        if (operation.ExchangeClient is not null)
        {
            site = scope.SelectSite(operation,
                arguments.TryGetProperty("site", out var suppliedSite) && suppliedSite.ValueKind == JsonValueKind.String
                    ? suppliedSite.GetString()
                    : null);
        }

        var schema = operation.Schema(scope.Sites(operation), scope.InferSite && scope.Sites(operation).Length == 1);
        var validation = Validate(schema, arguments);

        if (!validation.IsValid)
        {
            throw new ArgumentException("Invalid arguments: " + string.Join("; ", Errors(validation).Distinct().Take(20)));
        }

        // Match against each original overload as well as the merged public schema. This rejects
        // combinations such as a zone leaderboard with paging before making any HTTP request.
        var libraryArguments = JsonNode.Parse(arguments.GetRawText())!.AsObject();
        libraryArguments.Remove("site");
        libraryArguments.Remove("platform");

        using var boundDocument = JsonDocument.Parse(libraryArguments.ToJsonString());
        var boundArguments = boundDocument.RootElement;
        var overload = operation.Overloads.FirstOrDefault(candidate => Validate(candidate.Schema, boundArguments).IsValid);

        if (overload is null)
        {
            throw new ArgumentException("No overload accepts this argument combination. Valid combinations: " +
                string.Join("; ", operation.Overloads.Select(candidate => "(" + candidate.Signature + ")")) + ".");
        }

        using var activity = Activities.StartActivity(id);
        activity?.SetTag("maniaapi.operation", id);

        var started = Stopwatch.GetTimestamp();
        var limiter = audienceLimits.GetOrAdd(id.StartsWith("nadeo.", StringComparison.Ordinal)
            ? string.Join('.', id.Split('.').Take(2))
            : operation.Toolset, _ => new SemaphoreSlim(3, 3));

        await limiter.WaitAsync(cancellationToken);

        try
        {
            var resourceArguments = JsonNode.Parse(arguments.GetRawText())!.AsObject();

            if (site is not null)
            {
                resourceArguments["site"] = site;
            }

            using var resourceDocument = JsonDocument.Parse(resourceArguments.ToJsonString());
            var uri = operation.ReturnsBinary ? ResourceUri.Create(id, resourceDocument.RootElement) : null;
            var platform = arguments.TryGetProperty("platform", out var platformValue) ? platformValue.GetString() : null;

            var context = new InvocationContext(services, services.GetRequiredService<INadeoAccount>(),
                site, platform, uri, capture, operation);
            var raw = await overload.Invoke(context, boundArguments, cancellationToken);

            if (!operation.PlayerOnly)
            {
                health.Record(operation.Requires, authorized: true, operation.OAuthProvider);
            }

            if (IsSecret(raw))
            {
                raw = JsonValue.Create("[redacted]");
            }
            else
            {
                RemoveSecrets(raw);
            }

            FormattedText.AddPlain(raw);

            var result = ResultShaper.Shape(raw, shape, options.Shaping);
            activity?.SetTag("http.response.status_code", 200);
            ResultSize.Record(raw?.ToJsonString().Length ?? 0,
                new KeyValuePair<string, object?>("operation", id), new KeyValuePair<string, object?>("form", "raw"));
            ResultSize.Record(result.ToJsonString().Length,
                new KeyValuePair<string, object?>("operation", id), new KeyValuePair<string, object?>("form", "shaped"));

            return result;
        }
        catch (Exception exception)
        {
            var status = UpstreamStatus(exception);
            activity?.SetTag("http.response.status_code", status);
            activity?.SetStatus(ActivityStatusCode.Error);

            // Only a credential that can't authorize is unhealthy. Permission denials on single operations
            // and upstream outages (network errors, 5xx) are reported to the caller, not to readiness.
            if (!operation.PlayerOnly && status == 401)
            {
                health.RecordUnauthorized(operation.Requires, operation.OAuthProvider, services);
            }

            if (status == 401 && operation.ServiceAccountOnly)
            {
                throw new ArgumentException(
                    "Upstream authorization was denied. This operation needs a NadeoAPI service account; dedicated server logins are not allowed.",
                    exception);
            }

            throw;
        }
        finally
        {
            limiter.Release();
            Duration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", id));
        }
    }

    /// <summary>
    /// Whether this server offers the operation at all: its toolset, master server game, writes, and
    /// account linking are switched on. Disabled operations behave as if they don't exist.
    /// </summary>
    private bool IsEnabled(Operation operation, GameScope scope)
    {
        if (!scope.Contains(operation) || (!operation.ReadOnly && !options.AllowWrites))
        {
            return false;
        }

        if (operation.PlayerOnly)
        {
            return options.AccountLinking.Enabled;
        }

        if (operation.Id.StartsWith("masterserver.", StringComparison.Ordinal))
        {
            return options.MasterServer.Games.Contains(operation.Id.Split('.')[1] switch
            {
                "tmuf" => "TMUF",
                "tmnf" => "TMNF",
                "mp3" => "MP3",
                "mp4" => "MP4",
                _ => "TMT"
            });
        }

        return true;
    }

    /// <summary>
    /// Null when the server has the credentials the operation needs; otherwise which ones, naming the settings.
    /// </summary>
    private string? MissingCredentials(Operation operation)
    {
        var requires = operation.PlayerOnly
            ? operation.OAuthProvider == "trackmania" ? "trackmania" : "maniaplanet"
            : operation.Requires;

        return requires switch
        {
            "nadeo" when !options.NadeoAPI.Configured =>
                "NadeoAPI credentials (ManiaAPI:NadeoAPI:Login and Password)",
            "oauth" or "trackmania" when !options.TrackmaniaAPI.Configured =>
                "Trackmania web API credentials (ManiaAPI:TrackmaniaAPI:ClientId and ClientSecret)",
            "maniaplanet" when !options.ManiaPlanet.Configured =>
                "ManiaPlanet web API credentials (ManiaAPI:ManiaPlanet:ClientId and ClientSecret)",
            "tmws" when !options.TrackmaniaWS.Configured =>
                "Trackmania Web Services credentials (ManiaAPI:TrackmaniaWS:Username and Password)",
            _ => null
        };
    }

    public void ValidateArguments(JsonElement schema, JsonElement arguments)
    {
        var result = Validate(JsonNode.Parse(schema.GetRawText())!, arguments);

        if (!result.IsValid)
        {
            throw new ArgumentException("Invalid arguments: " + string.Join("; ", Errors(result).Distinct().Take(20)));
        }
    }

    private EvaluationResults Validate(JsonNode schema, JsonElement arguments)
    {
        var text = schema.ToJsonString();
        var compiled = schemas.GetOrAdd(text, value => JsonSchema.FromText(value));

        return compiled.Evaluate(JsonNode.Parse(arguments.GetRawText()), new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });
    }

    private static IEnumerable<string> Errors(EvaluationResults result)
    {
        if (result.Errors is not null)
        {
            var location = result.InstanceLocation.ToString();

            // The message names the problem, such as which required fields are missing; the keyword alone doesn't.
            foreach (var (keyword, message) in result.Errors)
            {
                yield return $"{(location.Length == 0 ? "arguments" : location)}: {(string.IsNullOrEmpty(message) ? keyword + " validation failed" : message)}";
            }
        }

        foreach (var detail in result.Details)
        {
            foreach (var error in Errors(detail))
            {
                yield return error;
            }
        }
    }

    private void RemoveSecrets(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(x => x.Key).ToArray())
            {
                var normalized = key.Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();

                if (normalized is "password" or "clientsecret" or "accesstoken" or "refreshtoken" or "authorization")
                {
                    obj.Remove(key);
                }
                else
                {
                    if (IsSecret(obj[key]))
                    {
                        obj[key] = "[redacted]";
                    }
                    else
                    {
                        RemoveSecrets(obj[key]);
                    }
                }
            }
        }
        else if (node is JsonArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                if (IsSecret(array[i]))
                {
                    array[i] = "[redacted]";
                }
                else
                {
                    RemoveSecrets(array[i]);
                }
            }
        }
    }

    private bool IsSecret(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) && sensitiveValues.Contains(text);
    }

    public static JsonObject Error(Exception exception)
    {
        var status = UpstreamStatus(exception);

        var retryable = exception is HttpRequestException && status is null ||
            exception is OperationCanceledException || status is >= 500 or 429;

        var message = exception switch
        {
            ArgumentException argument => argument.Message,
            MasterServer.XmlRequestException xml => $"Master server error {xml.Value}. Check the login, title, league, and game context.",
            JsonException => "Invalid argument value. Use maniaapi_describe to check the schema.",
            _ when status == 401 => "Upstream authorization was denied. Check the configured account and its permissions.",
            _ when status == 403 => "The configured account cannot access this upstream resource; club membership may be required.",
            _ when status == 404 => "The upstream resource was not found.",
            _ when status == 400 => "The upstream API rejected the request. Check its limits; display-name requests accept at most 50 names.",
            _ when retryable => "The upstream service is temporarily unavailable. Retry later.",
            _ => "The operation failed. Check the server configuration or upstream service."
        };

        return new JsonObject
        {
            ["message"] = message,
            ["retryable"] = retryable,
            ["upstreamStatus"] = status
        };
    }

    private static int? UpstreamStatus(Exception exception)
    {
        return exception switch
        {
            NadeoAPIResponseException nadeo => (int)nadeo.StatusCode,
            TrackmaniaAPIResponseException oauth => (int)oauth.StatusCode,
            ManiaPlanetAPI.ManiaPlanetAPIResponseException planet => (int)planet.StatusCode,
            HttpRequestException http => (int?)http.StatusCode,
            _ => null
        };
    }
}

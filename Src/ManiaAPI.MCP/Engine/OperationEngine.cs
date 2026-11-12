using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
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

        sensitiveValues = options.NadeoAPI.Accounts.Values
            .SelectMany(account => new[] { account.Login, account.Password })
            .Concat([options.TrackmaniaApi.ClientId, options.TrackmaniaApi.ClientSecret,
                options.ManiaPlanet.ClientId, options.ManiaPlanet.ClientSecret,
                options.TrackmaniaWS.ApiUsername, options.TrackmaniaWS.ApiPassword])
            .Concat(options.Http.ApiKeys)
            .Where(value => !string.IsNullOrEmpty(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.Ordinal);
    }

    public IEnumerable<Operation> Available(GameScope scope)
    {
        return operations.Values.Where(operation => scope.Contains(operation) && IsConfigured(operation) &&
            (operation.ReadOnly || options.AllowWrites));
    }

    public Operation Describe(string id, GameScope scope)
    {
        if (!operations.TryGetValue(id, out var operation) || !scope.Contains(operation))
        {
            throw new ArgumentException("Unknown operation in this endpoint's scope. Use maniaapi_search.");
        }

        if (!IsConfigured(operation))
        {
            throw new ArgumentException($"This operation needs {operation.Requires} credentials, which this server doesn't have.");
        }

        if (!operation.ReadOnly && !options.AllowWrites)
        {
            throw new ArgumentException("Writes are disabled on this server.");
        }

        return operation;
    }

    public async Task<JsonObject> InvokeAsync(
        string id,
        JsonElement arguments,
        GameScope scope,
        IServiceProvider services,
        ResultShape? shape = null,
        string? account = null,
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

        var effectiveAccount = SelectAccount(operation, account);
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
            throw new ArgumentException("No overload accepts this argument combination. Use maniaapi_describe for valid signatures.");
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

            var context = new InvocationContext(services, services.GetRequiredService<INadeoAccounts>(), effectiveAccount,
                site, platform, uri, capture, operation);
            var raw = await overload.Invoke(context, boundArguments, cancellationToken);

            if (!operation.PlayerOnly)
            {
                health.Record(operation.Requires, effectiveAccount, authorized: true, operation.OAuthProvider);
            }

            if (IsSecret(raw))
            {
                raw = JsonValue.Create("[redacted]");
            }
            else
            {
                RemoveSecrets(raw);
            }

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

            if (!operation.PlayerOnly && (status == 401 || exception is HttpRequestException { StatusCode: null }))
            {
                health.Record(operation.Requires, effectiveAccount, authorized: false, operation.OAuthProvider);
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

    private string? SelectAccount(Operation operation, string? requested)
    {
        if (operation.Requires != "nadeo")
        {
            if (requested is not null)
            {
                throw new ArgumentException("account is only supported for NadeoAPI operations.");
            }

            return null;
        }

        var selected = requested ?? (operation.ServiceAccountOnly ? "ServiceAccount" : options.NadeoAPI.DefaultAccount);

        if (requested is null && !operation.ServiceAccountOnly &&
            (!options.NadeoAPI.Accounts.TryGetValue(selected, out var preferred) || !preferred.Configured))
        {
            selected = options.NadeoAPI.Accounts.First(x => x.Value.Configured).Key;
        }

        if (operation.ServiceAccountOnly && selected != "ServiceAccount")
        {
            throw new ArgumentException("This operation needs a service account: pass account: ServiceAccount.");
        }

        if (!options.NadeoAPI.Accounts.TryGetValue(selected, out var credentials) || !credentials.Configured)
        {
            throw new ArgumentException("The selected NadeoAPI account is not configured.");
        }

        return selected;
    }

    private bool IsConfigured(Operation operation)
    {
        if (operation.PlayerOnly)
        {
            return options.AccountLinking.Enabled &&
                (operation.OAuthProvider == "trackmania" ? options.TrackmaniaApi.Configured : options.ManiaPlanet.Configured);
        }

        return operation.Requires switch
        {
            "nadeo" => operation.ServiceAccountOnly
                ? options.NadeoAPI.Accounts.TryGetValue("ServiceAccount", out var account) && account.Configured
                : options.NadeoAPI.Accounts.Values.Any(x => x.Configured),
            "oauth" => options.TrackmaniaApi.Configured,
            "tmws" => options.TrackmaniaWS.Configured,
            _ when operation.Id.StartsWith("masterserver.", StringComparison.Ordinal) => options.MasterServer.Games.Contains(
                operation.Id.Split('.')[1] switch
                {
                    "tmuf" => "TMUF",
                    "tmnf" => "TMNF",
                    "mp3" => "MP3",
                    "mp4" => "MP4",
                    _ => "TMT"
                }),
            _ => true
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
            foreach (var error in result.Errors.Keys)
            {
                yield return $"{result.InstanceLocation}: {error} validation failed";
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

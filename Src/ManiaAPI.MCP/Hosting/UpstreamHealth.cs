using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Catalog;

namespace ManiaAPI.MCP.Hosting;

public sealed class UpstreamHealth
{
    private readonly ConcurrentDictionary<string, string> states = new();

    public UpstreamHealth(McpOptions options)
    {
        if (options.NadeoAPI.Configured)
        {
            states["nadeo"] = "notYetAuthorized";
        }

        if (options.TrackmaniaAPI.Configured)
        {
            states["oauth:trackmania"] = "notYetAuthorized";
        }

        if (options.ManiaPlanet.Configured)
        {
            states["oauth:maniaplanet"] = "notYetAuthorized";
        }

        if (options.TrackmaniaWS.Configured)
        {
            states["tmws"] = "notYetAuthorized";
        }
    }

    public bool Ready => states.Values.All(value => value != "failed");

    public void Record(string requires, bool authorized, string? provider = null)
    {
        var key = Key(requires, provider);

        if (key is null || requires == "none" && !states.ContainsKey(key))
        {
            return;
        }

        states[key] = authorized ? "authorized" : "failed";
    }

    /// <summary>
    /// Records an upstream 401 as a credential failure only when the credential holds no valid token.
    /// A 401 while a token is valid means the account lacks permission for that one operation (for example
    /// a dedicated server login calling a service-account-only route); it says nothing about whether the
    /// credential works, so it must not take the instance out of rotation.
    /// </summary>
    public void RecordUnauthorized(string requires, string? provider, IServiceProvider services)
    {
        var key = Key(requires, provider);

        if (key is null || !states.ContainsKey(key))
        {
            return;
        }

        if (key != "tmws" && Expirations(key, services).Any(expiration => expiration > DateTimeOffset.UtcNow))
        {
            return;
        }

        states[key] = "failed";
    }

    private static string? Key(string requires, string? provider)
    {
        return requires switch
        {
            "nadeo" => "nadeo",
            "tmws" => "tmws",
            "oauth" => "oauth:" + (provider ?? "trackmania"),
            "none" when provider == "maniaplanet" => "oauth:maniaplanet",
            _ => null
        };
    }

    private static DateTimeOffset[] Expirations(string key, IServiceProvider services)
    {
        return key switch
        {
            "nadeo" => services.GetRequiredService<NadeoAccount>().Expirations().ToArray(),
            "oauth:trackmania" when services.GetRequiredService<TrackmaniaAPI.TrackmaniaAPIHandler>().Payload?.ExpirationTime is { } trackmania => [trackmania],
            "oauth:maniaplanet" when services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetAPIHandler>().Payload?.ExpirationTime is { } planet => [planet],
            _ => []
        };
    }

    public JsonObject Snapshot(IServiceProvider? services = null)
    {
        var snapshot = states.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        if (services is not null)
        {
            foreach (var key in snapshot.Keys.ToArray())
            {
                if (snapshot[key] == "failed" || key == "tmws")
                {
                    continue;
                }

                snapshot[key] = Expirations(key, services).Any(expiration => expiration > DateTimeOffset.UtcNow)
                    ? "authorized"
                    : "notYetAuthorized";
            }
        }

        return new JsonObject(snapshot.OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new KeyValuePair<string, JsonNode?>(x.Key, JsonValue.Create(x.Value))));
    }
}

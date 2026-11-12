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
        foreach (var account in options.Nadeo.Accounts.Where(x => x.Value.Configured))
        {
            states["nadeo:" + account.Key] = "notYetAuthorized";
        }

        if (options.TrackmaniaApi.Configured)
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

    public void Record(string requires, string? account, bool authorized, string? provider = null)
    {
        if (requires == "none" && provider != "maniaplanet")
        {
            return;
        }

        var key = requires == "nadeo" ? "nadeo:" + account : requires == "tmws" ? "tmws" : "oauth:" + (provider ?? "trackmania");

        if (requires == "none" && !states.ContainsKey(key))
        {
            return;
        }

        states[key] = authorized ? "authorized" : "failed";
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

                var expirations = key.StartsWith("nadeo:", StringComparison.Ordinal)
                    ? services.GetRequiredService<NadeoAccounts>().Expirations(key[6..]).ToArray()
                    : key == "oauth:trackmania" && services.GetRequiredService<TrackmaniaAPI.TrackmaniaAPIHandler>().Payload?.ExpirationTime is { } trackmania
                        ? [trackmania]
                        : key == "oauth:maniaplanet" && services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetAPIHandler>().Payload?.ExpirationTime is { } planet
                            ? [planet]
                            : Array.Empty<DateTimeOffset>();

                snapshot[key] = expirations.Any(expiration => expiration > DateTimeOffset.UtcNow)
                    ? "authorized"
                    : "notYetAuthorized";
            }
        }

        return new JsonObject(snapshot.OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new KeyValuePair<string, JsonNode?>(x.Key, JsonValue.Create(x.Value))));
    }
}

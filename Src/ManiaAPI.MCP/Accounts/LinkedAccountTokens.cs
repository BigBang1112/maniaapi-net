using System.Text.Json;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;

namespace ManiaAPI.MCP.Accounts;

public sealed class LinkedAccountTokens(
    LinkedAccountStore store,
    McpOptions options,
    IHttpClientFactory clients,
    TimeProvider clock)
{
    public async Task<string> GetAsync(string caller, string provider, string[] scopes, CancellationToken cancellationToken)
    {
        await using var lease = await store.LockAsync(caller, provider, cancellationToken);
        var tokens = await store.ReadAsync(caller, provider, cancellationToken)
            ?? throw new ArgumentException($"Link a player account first using POST /account-links/{provider} with your API key.");

        if (scopes.Except(tokens.Scopes, StringComparer.Ordinal).Any())
        {
            throw new ArgumentException("The linked player has not granted the required scopes. Link the account again with the scopes reported by maniaapi_describe.");
        }

        if (tokens.ExpiresAt > clock.GetUtcNow().AddMinutes(1))
        {
            return tokens.AccessToken;
        }

        if (string.IsNullOrWhiteSpace(tokens.RefreshToken))
        {
            throw new ArgumentException("The linked player token expired. Link the account again.");
        }

        var credentials = provider == "trackmania" ? options.TrackmaniaApi : options.ManiaPlanet;
        var endpoint = provider == "trackmania"
            ? TrackmaniaAuthenticationDefaults.TokenEndpoint
            : ManiaPlanetAuthenticationDefaults.TokenEndpoint;

        using var client = clients.CreateClient("LinkedAccountTokens");
        using var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = tokens.RefreshToken,
            ["client_id"] = credentials.ClientId!,
            ["client_secret"] = credentials.ClientSecret!,
            ["scope"] = string.Join(' ', tokens.Scopes)
        });

        using var response = await client.PostAsync(endpoint, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException("The player token could not be refreshed. Link the account again.");
        }

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        var root = document.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("The OAuth provider did not return an access token.");

        var updated = new LinkedTokens(
            accessToken,
            root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : tokens.RefreshToken,
            clock.GetUtcNow().AddSeconds(root.TryGetProperty("expires_in", out var expires) ? expires.GetDouble() : 3600),
            root.TryGetProperty("scope", out var granted) ? granted.GetString()!.Split(' ', StringSplitOptions.RemoveEmptyEntries) : tokens.Scopes);

        await store.WriteAsync(caller, provider, updated, cancellationToken);

        if (scopes.Except(updated.Scopes, StringComparer.Ordinal).Any())
        {
            throw new ArgumentException("The refreshed player token lacks the required scopes. Link the account again.");
        }

        return accessToken;
    }
}

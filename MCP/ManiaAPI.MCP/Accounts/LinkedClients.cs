using System.Net.Http.Headers;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.ManiaPlanetAPI;
using ManiaAPI.TrackmaniaAPI;

namespace ManiaAPI.MCP.Accounts;

public sealed class LinkedClients(
    LinkedAccountTokens tokens,
    IHttpClientFactory clients,
    IHttpContextAccessor httpContext) : IDisposable
{
    private readonly List<IDisposable> owned = [];

    public async Task<object> GetAsync(Operation operation, CancellationToken cancellationToken)
    {
        var caller = httpContext.HttpContext?.Items["ManiaApiIdentity"] as string
            ?? throw new ArgumentException("Player-linked operations require an authenticated HTTP caller.");

        var accessToken = await tokens.GetAsync(caller, operation.OAuthProvider!, operation.OAuthScopes, cancellationToken);
        var http = clients.CreateClient("LinkedPlayer");
        owned.Add(http);

        if (operation.OAuthProvider == "trackmania")
        {
            return new TrackmaniaAPI.TrackmaniaAPI(http, new TrackmaniaAPIHandler
            {
                Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
            }, automaticallyAuthorize: false);
        }

        return new ManiaPlanetAPI.ManiaPlanetAPI(http, new ManiaPlanetAPIHandler
        {
            Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
        }, automaticallyAuthorize: false);
    }

    public void Dispose()
    {
        foreach (var item in owned)
        {
            item.Dispose();
        }
    }
}

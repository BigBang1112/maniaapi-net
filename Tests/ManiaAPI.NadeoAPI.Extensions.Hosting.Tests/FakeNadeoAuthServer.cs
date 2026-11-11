using System.Collections.Concurrent;
using System.Text.Json;
using ManiaAPI.NadeoAPI.Tests;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests;

/// <summary>
/// Fakes the Nadeo authentication endpoints. Authorizing returns an already expired token, so the next refresh sends a request.
/// The refresh token is the audience, which tells apart whose refresh a request is.
/// </summary>
internal sealed class FakeNadeoAuthServer : HttpMessageHandler
{
    private const string BasicPath = "/v2/authentication/token/basic";
    private const string RefreshPath = "/v2/authentication/token/refresh";

    private readonly ConcurrentQueue<string> refreshes = new();

    /// <summary>
    /// Answers a refresh for the given audience. Refreshes succeed by default.
    /// </summary>
    public Func<string, CancellationToken, Task<HttpResponseMessage>> RespondToRefresh { get; set; } =
        (audience, _) => Task.FromResult(ValidToken(audience));

    /// <summary>
    /// Audiences of the refresh requests, in the order they were sent.
    /// </summary>
    public IReadOnlyCollection<string> Refreshes => refreshes;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        switch (request.RequestUri!.AbsolutePath)
        {
            case BasicPath:
                var body = await request.Content!.ReadAsStringAsync(cancellationToken);
                var audience = JsonDocument.Parse(body).RootElement.GetProperty("audience").GetString()!;
                return ExpiredToken(audience);

            case RefreshPath:
                var refreshAudience = request.Headers.Authorization!.Parameter!["t=".Length..];
                refreshes.Enqueue(refreshAudience);
                return await RespondToRefresh(refreshAudience, cancellationToken);

            default:
                throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        }
    }

    public static HttpResponseMessage ValidToken(string audience)
    {
        var accessToken = NadeoTokens.AccessToken(audience, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddMinutes(30));
        return FakeResponse.Json(NadeoTokens.Response(accessToken, refreshToken: audience));
    }

    private static HttpResponseMessage ExpiredToken(string audience)
    {
        var accessToken = NadeoTokens.AccessToken(audience, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(-2));
        return FakeResponse.Json(NadeoTokens.Response(accessToken, refreshToken: audience));
    }
}

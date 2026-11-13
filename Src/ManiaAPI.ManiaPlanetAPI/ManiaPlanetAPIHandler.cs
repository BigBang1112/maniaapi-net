using System.Net.Http.Headers;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed class ManiaPlanetAPIHandler
{
    public ManiaPlanetAPICredentials? Credentials { get; set; }

    /// <summary>
    /// The authorization sent with each request. It is set by authorizing, or can be set to a bearer access token obtained
    /// elsewhere, such as through the authorization code flow, such a client can't re-authorize when the token expires.
    /// </summary>
    public AuthenticationHeaderValue? Authorization { get; set; }

    internal SemaphoreSlim AuthorizationLock { get; } = new(1, 1);

    /// <summary>
    /// The decoded payload of the access token from the last authorization, including its expiration time.
    /// </summary>
    public JwtPayloadManiaPlanetAPI? Payload { get; internal set; }
}

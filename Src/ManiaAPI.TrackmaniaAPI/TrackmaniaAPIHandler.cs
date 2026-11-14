using System.Net.Http.Headers;

namespace ManiaAPI.TrackmaniaAPI;

public sealed class TrackmaniaAPIHandler
{
    /// <summary>
    /// The authorization sent with each request. It is set by authorizing, or can be set to a bearer access token obtained
    /// elsewhere, such as through the authorization code flow, such a client can't re-authorize when the token expires.
    /// </summary>
    public AuthenticationHeaderValue? Authorization { get; set; }

    /// <summary>
    /// Held while authorizing. It lives here, not in the client, because every client sharing this handler
    /// (like the transient clients from DI) must take turns, otherwise each of them authorizes at once.
    /// </summary>
    internal SemaphoreSlim AuthorizationLock { get; } = new(1, 1);

    public TrackmaniaAPICredentials? Credentials { get; set; }

    public JwtPayloadTrackmaniaAPI? Payload { get; internal set; }
}

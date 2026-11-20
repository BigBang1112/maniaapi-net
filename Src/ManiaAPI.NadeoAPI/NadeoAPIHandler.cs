using System.Net.Http.Headers;

namespace ManiaAPI.NadeoAPI;

public sealed class NadeoAPIHandler
{
    /// <summary>
    /// Credentials used for initial authentication. They are discarded after the first successful authentication.
    /// If Nadeo rejects them with HTTP 401, requests fail with that error without signing in again until new credentials are set here.
    /// </summary>
    public NadeoAPICredentials? PendingCredentials { get; set; }

    /// <summary>
    /// Credentials Nadeo rejected with HTTP 401 and its error. Signing in again with the same credentials can't succeed
    /// and could get the account locked, so requests fail with this error instead while they're still pending.
    /// </summary>
    internal (NadeoAPICredentials Credentials, NadeoAPIResponseException Error)? CredentialsRejection { get; set; }

    /// <summary>
    /// Credentials used for maintaining authentication after refresh token expires. This is only used if <see cref="SaveCredentials"/> is set to true.
    /// </summary>
    internal NadeoAPICredentials? SavedCredentials { get; set; }

    internal AuthenticationHeaderValue? Authorization { get; set; }

    /// <summary>
    /// Held while authorizing or refreshing. It lives here, not in the client, because every client sharing this handler
    /// (like the transient clients from DI) must take turns, otherwise they authorize or refresh with the same refresh token at once.
    /// </summary>
    internal SemaphoreSlim AuthorizationLock { get; } = new(1, 1);

    internal RateLimiter RequestLimiter { get; init; } = RateLimiter.Shared;

    internal string? RefreshToken { get; set; }

    public JwtPayloadNadeoAPI? JWT { get; internal set; }
    [Obsolete("Ubisoft account authentication flow is no longer working, so this property is no longer used")]
    public UbisoftAuthenticationTicket? UbisoftTicket { get; internal set; }

    /// <summary>
    /// Whether to save credentials after successful authentication, so the client can sign in again once the refresh token
    /// has expired (after about 24 hours of inactivity) or was rejected. This is true by default, and the ManiaAPI.NadeoAPI.Extensions.Hosting
    /// package keeps it on. In case you don't need 24/7 authentication, you can set this to false so the credentials aren't kept in memory.
    /// </summary>
    public bool SaveCredentials { get; init; } = true;
}

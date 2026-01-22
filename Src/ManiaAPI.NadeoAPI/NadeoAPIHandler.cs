using System.Net.Http.Headers;

namespace ManiaAPI.NadeoAPI;

public sealed class NadeoAPIHandler
{
    /// <summary>
    /// Credentials used for initial authentication. They are discarded after the first successful authentication.
    /// </summary>
    public NadeoAPICredentials? PendingCredentials { get; set; }

    /// <summary>
    /// Credentials used for maintaining authentication after refresh token expires. This is only used if <see cref="SaveCredentials"/> is set to true.
    /// </summary>
    internal NadeoAPICredentials? SavedCredentials { get; set; }

    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal string? RefreshToken { get; set; }

    public JwtPayloadNadeoAPI? JWT { get; internal set; }
    public UbisoftAuthenticationTicket? UbisoftTicket { get; internal set; }

    /// <summary>
    /// Whether to save credentials after successful authentication. This is true by default to reduce confusion regarding refresh tokens after 24 hours of inactivity, but in case you don't need 24/7 authentication, you should set this to false to increase security. The ManiaAPI.NadeoAPI.Extensions.Hosting package solves this problem differently, so this property is ignored when adding the client via that package.
    /// </summary>
    public bool SaveCredentials { get; set; } = true;
}

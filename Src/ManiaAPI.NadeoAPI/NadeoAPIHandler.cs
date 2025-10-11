using System.Net.Http.Headers;

namespace ManiaAPI.NadeoAPI;

public sealed class NadeoAPIHandler
{
    /// <summary>
    /// Credentials used for initial authentication. They are discarded after the first successful authentication.
    /// </summary>
    public NadeoAPICredentials? PendingCredentials { get; set; }

    /// <summary>
    /// Credentials used for maintaining authentication after refresh token expires.
    /// </summary>
    public NadeoAPICredentials? SavedCredentials { get; set; }

    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal string? RefreshToken { get; set; }

    public JwtPayloadNadeoAPI? JWT { get; internal set; }
    public UbisoftAuthenticationTicket? UbisoftTicket { get; internal set; }
}

using System.Net.Http.Headers;

namespace ManiaAPI.NadeoAPI;

public sealed class NadeoAPIHandler
{
    public NadeoAPICredentials? PendingConnection { get; set; }

    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal string? RefreshToken { get; set; }

    public JwtPayloadNadeoAPI? JWT { get; internal set; }
    public UbisoftAuthenticationTicket? UbisoftTicket { get; internal set; }
}

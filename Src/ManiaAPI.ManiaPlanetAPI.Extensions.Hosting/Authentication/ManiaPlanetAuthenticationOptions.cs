using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

public class ManiaPlanetAuthenticationOptions : OAuthOptions
{
    public string EmailEndpoint { get; set; } = ManiaPlanetAuthenticationDefaults.EmailEndpoint;

    /// <summary>Application User-Agent sent first on OAuth backchannel requests.</summary>
    public string? UserAgent { get; set; }

    public ManiaPlanetAuthenticationOptions()
    {
        ClaimsIssuer = ManiaPlanetAuthenticationDefaults.Issuer;
        CallbackPath = ManiaPlanetAuthenticationDefaults.CallbackPath;
        AuthorizationEndpoint = ManiaPlanetAuthenticationDefaults.AuthorizationEndpoint;
        TokenEndpoint = ManiaPlanetAuthenticationDefaults.TokenEndpoint;
        UserInformationEndpoint = ManiaPlanetAuthenticationDefaults.UserInformationEndpoint;

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "login");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
        ClaimActions.MapJsonKey(ManiaPlanetAuthenticationConstants.Claims.Zone, "path");
    }
}

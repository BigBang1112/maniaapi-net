using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

public class ManiaPlanetAuthenticationOptions : OAuthOptions
{
    public ManiaPlanetAuthenticationOptions()
    {
        ClaimsIssuer = ManiaPlanetAuthenticationDefaults.Issuer;
        CallbackPath = ManiaPlanetAuthenticationDefaults.CallbackPath;
        AuthorizationEndpoint = ManiaPlanetAuthenticationDefaults.AuthorizationEndpoint;
        TokenEndpoint = ManiaPlanetAuthenticationDefaults.TokenEndpoint;
        UserInformationEndpoint = ManiaPlanetAuthenticationDefaults.UserInformationEndpoint;
        SaveTokens = true;

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "login");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
        ClaimActions.MapJsonKey(ManiaPlanetAuthenticationConstants.Claims.Zone, "path");
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;

public class TrackmaniaAuthenticationOptions : OAuthOptions
{
    public TrackmaniaAuthenticationOptions()
    {
        ClaimsIssuer = TrackmaniaAuthenticationDefaults.Issuer;
        CallbackPath = TrackmaniaAuthenticationDefaults.CallbackPath;
        AuthorizationEndpoint = TrackmaniaAuthenticationDefaults.AuthorizationEndpoint;
        TokenEndpoint = TrackmaniaAuthenticationDefaults.TokenEndpoint;
        UserInformationEndpoint = TrackmaniaAuthenticationDefaults.UserInformationEndpoint;

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "accountId");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "displayName");
    }
}

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

public static class ManiaPlanetAuthenticationDefaults
{
    public const string AuthenticationScheme = "ManiaPlanet";
    public const string DisplayName = "ManiaPlanet";
    public const string Issuer = "ManiaPlanet";
    public const string CallbackPath = "/signin-maniaplanet";
    public const string AuthorizationEndpoint = "https://prod.live.maniaplanet.com/login/oauth2/authorize";
    public const string TokenEndpoint = "https://prod.live.maniaplanet.com/login/oauth2/access_token";
    public const string UserInformationEndpoint = "https://prod.live.maniaplanet.com/webservices/me";
}

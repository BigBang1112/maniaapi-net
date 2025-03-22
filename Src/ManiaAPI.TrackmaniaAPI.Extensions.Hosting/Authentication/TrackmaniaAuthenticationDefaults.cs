namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;

public static class TrackmaniaAuthenticationDefaults
{
    public const string AuthenticationScheme = "Trackmania";
    public const string DisplayName = "Trackmania";
    public const string Issuer = "Trackmania";
    public const string CallbackPath = "/signin-trackmania";
    public const string AuthorizationEndpoint = "https://api.trackmania.com/oauth/authorize";
    public const string TokenEndpoint = "https://api.trackmania.com/api/access_token";
    public const string UserInformationEndpoint = "https://api.trackmania.com/api/user";
}

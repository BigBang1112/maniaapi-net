namespace ManiaAPI.NadeoAPI;

public sealed record NadeoAPICredentials(string Login, string Password)
{
    [Obsolete("Use the constructor with only Login and Password parameters instead. The Ubisoft account authentication method is no longer working (create a service account instead: https://www.trackmania.com/player/service-account)")]
    public NadeoAPICredentials(string Login, string Password, AuthorizationMethod Method) : this(Login, Password) { }
}

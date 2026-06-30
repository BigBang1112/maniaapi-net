namespace ManiaAPI.NadeoAPI;

[Obsolete("Ubisoft account authentication flow is no longer working, so now everything falls back to authentication that dedicated server uses")]
public enum AuthorizationMethod
{
    UbisoftAccount,
    DedicatedServer
}

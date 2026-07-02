namespace ManiaAPI.NadeoAPI;

[Obsolete("Ubisoft account authentication flow is no longer working, so this record class is no longer used")]
public sealed record UbisoftAuthenticationTicket(string PlatformType,
                                                 string Ticket,
                                                 string? TwoFactorAuthenticationTicket,
                                                 Guid ProfileId,
                                                 Guid UserId,
                                                 string NameOnPlatform,
                                                 string Environment,
                                                 DateTimeOffset Expiration,
                                                 Guid SpaceId,
                                                 string ClientIp,
                                                 string ClientIpCountry,
                                                 DateTimeOffset ServerTime,
                                                 Guid SessionId,
                                                 string SessionKey,
                                                 string? RememberMeTicket);

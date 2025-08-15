namespace ManiaAPI.NadeoAPI;

public sealed record WebIdentity(Guid AccountId,
                                 string Provider,
                                 Guid Uid,
                                 DateTimeOffset Timestamp);
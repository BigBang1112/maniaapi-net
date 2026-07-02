namespace ManiaAPI.NadeoAPI;

public sealed record SkinIdentifier(Guid AccountId,
                                    Guid SkinId,
                                    string SkinType,
                                    DateTimeOffset Timestamp);
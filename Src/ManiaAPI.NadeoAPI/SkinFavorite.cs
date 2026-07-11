namespace ManiaAPI.NadeoAPI;

public sealed record SkinFavorite(Guid AccountId, Guid SkinId, DateTimeOffset Timestamp);

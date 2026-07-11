namespace ManiaAPI.NadeoAPI;

public sealed record MapFavorite(Guid AccountId, string MapUid, DateTimeOffset Timestamp);

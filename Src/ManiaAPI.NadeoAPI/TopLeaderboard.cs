namespace ManiaAPI.NadeoAPI;

public sealed record TopLeaderboard(Guid ZoneId, string ZoneName, Record[] Top);
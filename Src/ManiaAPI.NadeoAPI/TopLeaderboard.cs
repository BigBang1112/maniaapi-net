namespace ManiaAPI.NadeoAPI;

public record TopLeaderboard(Guid ZoneId, string ZoneName, Record[] Top);
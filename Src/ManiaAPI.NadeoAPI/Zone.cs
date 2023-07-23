namespace ManiaAPI.NadeoAPI;

public sealed record Zone(string Icon, string Name, Guid? ParentId, DateTimeOffset Timestamp, Guid ZoneId);
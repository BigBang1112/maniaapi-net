using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public sealed record WorldRecord(int Id, string Group, Map Map, Player Player, DateTimeOffset DrivenAt, TimeInt32 Time, int TimeDiff, int Type);
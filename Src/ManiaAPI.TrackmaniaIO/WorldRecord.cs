using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public record WorldRecord(Map Map, Player Player, DateTimeOffset DrivenAt, TimeInt32 Time, int TimeDiff);
using TmEssentials;

namespace ManiaAPI.Xml.MP4;

public sealed record MapSummary(
    string MapUid, 
    string Zone, 
    MapLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    RecordUnit<TimeInt32>[] Skillpoints, 
    LeaderboardItem<TimeInt32>[] HighScores);

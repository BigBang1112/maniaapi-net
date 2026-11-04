using System.Collections.Immutable;
using TmEssentials;

namespace ManiaAPI.MasterServer.MP4;

public sealed record MapSummary(
    string MapUid, 
    string Zone, 
    MapLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    ImmutableArray<RecordUnit<TimeInt32>> Skillpoints,
    ImmutableArray<LeaderboardItem<TimeInt32>> HighScores);

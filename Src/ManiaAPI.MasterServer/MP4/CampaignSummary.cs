using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.MP4;

public sealed record CampaignSummary(
    string CampaignId, 
    string Zone, 
    CampaignLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    ImmutableArray<RecordUnit<uint>> Skillpoints, 
    ImmutableArray<LeaderboardItem<uint>> HighScores);

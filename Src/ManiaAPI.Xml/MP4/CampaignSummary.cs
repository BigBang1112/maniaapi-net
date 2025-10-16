using System.Collections.Immutable;

namespace ManiaAPI.Xml.MP4;

public sealed record CampaignSummary(
    string CampaignId, 
    string Zone, 
    CampaignLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    ImmutableArray<RecordUnit<uint>> Skillpoints, 
    ImmutableArray<LeaderboardItem<uint>> HighScores);

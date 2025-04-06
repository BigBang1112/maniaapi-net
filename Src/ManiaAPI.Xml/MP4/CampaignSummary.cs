namespace ManiaAPI.Xml.MP4;

public sealed record CampaignSummary(
    string CampaignId, 
    string Zone, 
    CampaignLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    RecordUnit<uint>[] Skillpoints, 
    LeaderboardItem<uint>[] HighScores);

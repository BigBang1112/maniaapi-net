namespace ManiaAPI.XmlRpc.MP4;

public sealed record CampaignSummary(
    string CampaignId, 
    string Zone, 
    CampaignLeaderboardType Type, 
    DateTimeOffset Timestamp, 
    int Count, 
    RecordUnit<uint>[] AllRecords, 
    LeaderboardItem<uint>[] Records);

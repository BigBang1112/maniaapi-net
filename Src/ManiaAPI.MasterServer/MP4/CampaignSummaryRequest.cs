namespace ManiaAPI.MasterServer.MP4;

public sealed record CampaignSummaryRequest(string? CampaignId = null, string Zone = "World", CampaignLeaderboardType Type = CampaignLeaderboardType.SkillPoint);

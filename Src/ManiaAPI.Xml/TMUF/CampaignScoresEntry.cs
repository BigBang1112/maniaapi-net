namespace ManiaAPI.Xml.TMUF;

public sealed record CampaignScoresEntry(string Zone, DateTimeOffset Timestamp, int Type, string FilePath, string Url);

namespace ManiaAPI.Xml.MP4;

public sealed record MapSummaryRequest(string MapUid, string Zone = "World", string Context = "", MapLeaderboardType Type = MapLeaderboardType.MapRecord);

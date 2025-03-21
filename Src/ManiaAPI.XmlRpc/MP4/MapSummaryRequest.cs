namespace ManiaAPI.XmlRpc.MP4;

public sealed record MapSummaryRequest(string MapUid, string Zone = "World", MapLeaderboardType Type = MapLeaderboardType.MapRecord);

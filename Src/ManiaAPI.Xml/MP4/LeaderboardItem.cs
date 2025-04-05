namespace ManiaAPI.Xml.MP4;

public record LeaderboardItem<T>(int Rank, string Login, string Nickname, T Score, string FileName, string DownloadUrl) where T : struct;

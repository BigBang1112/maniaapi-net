namespace ManiaAPI.XmlRpc.TMUF;

public sealed record PlayerRankings(int Count, IReadOnlyCollection<PlayerRanking> Players);
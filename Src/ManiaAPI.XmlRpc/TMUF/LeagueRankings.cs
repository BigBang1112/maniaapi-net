namespace ManiaAPI.XmlRpc.TMUF;

public sealed record LeagueRankings(int Count, IReadOnlyCollection<LeagueRanking> Leagues);
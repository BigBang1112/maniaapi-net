using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMUF;

public sealed record LeagueRankings(int Count, ImmutableArray<LeagueRanking> Leagues);
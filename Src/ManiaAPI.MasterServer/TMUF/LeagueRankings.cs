using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMUF;

public sealed record LeagueRankings(int Count, ImmutableList<LeagueRanking> Leagues);
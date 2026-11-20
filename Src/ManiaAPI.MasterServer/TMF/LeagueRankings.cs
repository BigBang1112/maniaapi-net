using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMF;

public sealed record LeagueRankings(int Count, ImmutableList<LeagueRanking> Leagues);
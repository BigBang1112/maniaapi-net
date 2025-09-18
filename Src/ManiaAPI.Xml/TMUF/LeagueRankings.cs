using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed record LeagueRankings(int Count, ImmutableList<LeagueRanking> Leagues);
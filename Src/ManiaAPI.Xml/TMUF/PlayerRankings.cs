using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed record PlayerRankings(int Count, ImmutableList<PlayerRanking> Players);
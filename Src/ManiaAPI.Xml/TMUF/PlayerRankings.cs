using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed record PlayerRankings(int Count, ImmutableArray<PlayerRanking> Players);
using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMUF;

public sealed record PlayerRankings(int Count, ImmutableArray<PlayerRanking> Players);
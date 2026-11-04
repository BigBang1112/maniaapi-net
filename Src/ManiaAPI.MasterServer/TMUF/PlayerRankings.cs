using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMUF;

public sealed record PlayerRankings(int Count, ImmutableList<PlayerRanking> Players);
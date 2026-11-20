using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMF;

public sealed record PlayerRankings(int Count, ImmutableList<PlayerRanking> Players);
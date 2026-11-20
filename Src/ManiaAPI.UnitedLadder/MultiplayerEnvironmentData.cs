using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record MultiplayerEnvironmentData(double Points, int Wins, int Losses, ImmutableList<RankInfo> Ranks);

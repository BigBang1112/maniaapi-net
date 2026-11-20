using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record SoloData(string Environment, double Points, ImmutableList<RankInfo> Ranks);

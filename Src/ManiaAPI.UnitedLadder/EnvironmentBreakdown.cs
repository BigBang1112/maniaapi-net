using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record EnvironmentBreakdown(long TotalPoints, int MapCount, double AveragePoints,
                                          ImmutableList<MapPoints> TopMaps, ImmutableList<MapPoints> AllMaps);

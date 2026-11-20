using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record CampaignBreakdown(long TotalPoints, int MapCount, double AveragePoints,
                                       ImmutableList<string> Environments, int EnvironmentCount,
                                       ImmutableList<MapPoints> TopMaps, ImmutableList<MapPoints> AllMaps);

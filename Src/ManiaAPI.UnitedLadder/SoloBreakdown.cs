using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record SoloBreakdown(ImmutableDictionary<string, EnvironmentBreakdown> Environments,
                                   ImmutableDictionary<string, CampaignBreakdown> Campaigns,
                                   BreakdownSummary Summary);

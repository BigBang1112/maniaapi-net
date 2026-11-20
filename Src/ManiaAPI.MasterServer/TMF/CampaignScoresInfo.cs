using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMF;

public sealed record CampaignScoresInfo(string Name, ImmutableList<CampaignScoresLeague> Leagues);

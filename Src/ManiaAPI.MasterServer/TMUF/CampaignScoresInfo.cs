using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMUF;

public sealed record CampaignScoresInfo(string Name, ImmutableList<CampaignScoresLeague> Leagues);

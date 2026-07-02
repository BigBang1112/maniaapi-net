using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed record CampaignScoresInfo(string Name, ImmutableList<CampaignScoresLeague> Leagues);

using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaignCollection(ImmutableList<ClubCampaign> ClubCampaignList,
                                            int MaxPage,
                                            int ItemCount);

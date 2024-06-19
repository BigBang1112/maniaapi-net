using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaignCollection(ImmutableArray<ClubCampaign> ClubCampaignList,
                                            int MaxPage,
                                            int ItemCount);

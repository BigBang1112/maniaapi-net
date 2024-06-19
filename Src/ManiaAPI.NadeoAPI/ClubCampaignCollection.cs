using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaignCollection(ImmutableArray<Campaign> ClubCampaignList,
                                            int MaxPage,
                                            int ItemCount);

using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucketItem(Guid ItemId,
                                    int Position,
                                    string Description,
                                    ImmutableList<string> MediaUrls,
                                    ImmutableList<string> MediaUrlsJpgLarge,
                                    ImmutableList<string> MediaUrlsJpgMedium,
                                    ImmutableList<string> MediaUrlsJpgSmall,
                                    ImmutableList<string> MediaUrlsDds);
using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucketItem(Guid ItemId,
                                    int Position,
                                    string Description,
                                    ImmutableArray<string> MediaUrls,
                                    ImmutableArray<string> MediaUrlsJpgLarge,
                                    ImmutableArray<string> MediaUrlsJpgMedium,
                                    ImmutableArray<string> MediaUrlsJpgSmall,
                                    ImmutableArray<string> MediaUrlsDds);
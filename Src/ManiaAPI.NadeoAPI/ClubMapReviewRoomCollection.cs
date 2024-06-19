using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMapReviewRoomCollection(ImmutableArray<ClubMapReviewRoom> ClubMapReviewList,
                                                 int MaxPage,
                                                 int ItemCount);
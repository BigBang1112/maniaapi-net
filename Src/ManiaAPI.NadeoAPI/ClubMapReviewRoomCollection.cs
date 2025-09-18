using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMapReviewRoomCollection(ImmutableList<ClubMapReviewRoom> ClubMapReviewList,
                                                 int MaxPage,
                                                 int ItemCount);
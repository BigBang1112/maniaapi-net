using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRoomCollection(ImmutableArray<ClubRoom> ClubRoomList,
                                        int MaxPage,
                                        int ItemCount);

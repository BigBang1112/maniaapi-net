using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRoomCollection(ImmutableList<ClubRoom> ClubRoomList,
                                        int MaxPage,
                                        int ItemCount);

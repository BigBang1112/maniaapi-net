using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubRoomCollection(ImmutableList<ClubRoomItem> Rooms, int Page, int PageCount);

using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubRoomCollection(ImmutableArray<ClubRoomItem> Rooms, int Page, int PageCount);

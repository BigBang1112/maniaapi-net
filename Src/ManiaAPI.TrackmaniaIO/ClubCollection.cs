using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubCollection(ImmutableArray<Club> Clubs, int Page, int PageCount);
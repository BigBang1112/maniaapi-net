using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubCollection(ImmutableList<Club> Clubs, int Page, int PageCount);
using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI;

internal sealed record AdminClubCollection(ImmutableList<AdminClub> Clubs);

using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI;

/// <summary>
/// A club the authenticated player is an admin of.
/// </summary>
public sealed record AdminClub(int Id, string Name);

internal sealed record AdminClubCollection(ImmutableList<AdminClub> Clubs);

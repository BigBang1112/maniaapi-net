using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaignCreation
{
    public required string Name { get; init; }
    public ImmutableList<ClubCampaignPlaylistItem>? Playlist { get; init; }
    public int? FolderId { get; init; }
}

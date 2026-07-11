namespace ManiaAPI.NadeoAPI;

public sealed record ClubRankingCreation
{
    public required string Name { get; init; }
    public required string UseCase { get; init; }
    public int? CampaignId { get; init; }
    public int? FolderId { get; init; }
}

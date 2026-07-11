namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionListItem(int ActivityId, int ClubId, int MaxPlayers, string Type, string Name, string? LogoUrl, string? VerticalUrl);

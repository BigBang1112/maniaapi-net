namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionInfo(int Id, int ClubId, int ActivityId, int MaxPlayers, string Type, string ParticipantAccess, Competition Competition, string? ExternalRegistrationUrl);

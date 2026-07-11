namespace ManiaAPI.NadeoAPI;

public sealed record MatchParticipant(Guid Participant, int Position, int? TeamPosition, int? Rank, int? Score, bool Mvp, string? Leaver, bool Eliminated);

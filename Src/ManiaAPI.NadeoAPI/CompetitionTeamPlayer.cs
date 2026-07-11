using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionTeamPlayer([property: JsonPropertyName("AccountId")] Guid AccountId);

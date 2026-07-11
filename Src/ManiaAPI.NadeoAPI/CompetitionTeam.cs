using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionTeam([property: JsonPropertyName("Id")] Guid Id,
                                     [property: JsonPropertyName("Name")] string Name,
                                     [property: JsonPropertyName("Players")] ImmutableList<CompetitionTeamPlayer> Players);

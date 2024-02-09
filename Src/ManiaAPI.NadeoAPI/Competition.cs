using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record Competition(int Id,
                                 string LiveId,
                                 Guid Creator,
                                 string Name,
                                 string ParticipantType,
                                 string? Description,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset MatchesGenerationDate,
                                 int NbPlayers,
                                 string SpotStructure,
                                 int LeaderboardId,
                                 string? Manialink,
                                 string? RulesUrl,
                                 string? StreamUrl,
                                 string? WebsiteUrl,
                                 string? LogoUrl,
                                 string? VerticalUrl,
                                 string[] AllowedZones,
                                 bool AutoNotmalizeSeeds,
                                 string Region,
                                 string AutoGetParticipantSkillLevel,
                                 string MatchAutoMode,
                                 string Partition);
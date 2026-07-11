using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
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
                                 [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? MatchesGenerationDate,
                                 int NbPlayers,
                                 string SpotStructure,
                                 int LeaderboardId,
                                 string? Manialink,
                                 string? RulesUrl,
                                 string? StreamUrl,
                                 string? WebsiteUrl,
                                 string? LogoUrl,
                                 string? VerticalUrl,
                                 ImmutableList<string> AllowedZones,
                                 bool AutoNotmalizeSeeds,
                                 string? Region,
                                 string AutoGetParticipantSkillLevel,
                                 string MatchAutoMode,
                                 string Partition,
                                 int? ActivityId = null,
                                 int? ClubId = null,
                                 [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? RegistrationStart = null,
                                 [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? RegistrationEnd = null,
                                 [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? DeletedOn = null);
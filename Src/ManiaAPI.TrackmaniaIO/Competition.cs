using ManiaAPI.TrackmaniaIO.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Competition(int Id,
                                 [property: JsonPropertyName("numplayers")] int NumPlayers,
                                 [property: JsonPropertyName("liveid")] string LiveId,
                                 [property: JsonPropertyName("clubid")] int ClubId,
                                 [property: JsonPropertyName("clubname")] string ClubName,
                                 [property: JsonPropertyName("creatorplayer")] Player CreatorPlayer,
                                 string Name,
                                 string Description,
                                 [property: JsonPropertyName("registrationstart"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset RegistrationStart,
                                 [property: JsonPropertyName("registrationend"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset RegistrationEnd,
                                 [property: JsonPropertyName("startdate"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                 [property: JsonPropertyName("enddate"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                                 [property: JsonPropertyName("leaderboardid")] int LeaderboardId,
                                 string Manialink,
                                 [property: JsonPropertyName("rulesurl")] string RulesUrl,
                                 [property: JsonPropertyName("streamurl")] string StreamUrl,
                                 [property: JsonPropertyName("websiteurl")] string WebsiteUrl,
                                 [property: JsonPropertyName("logourl")] string LogoUrl,
                                 [property: JsonPropertyName("verticalurl")] string VerticalUrl,
                                 ImmutableList<CompetitionRound> Rounds);
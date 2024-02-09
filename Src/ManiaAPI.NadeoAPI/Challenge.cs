using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record Challenge(int Id,
                               string Uid,
                               string Name,
                               string ScoreDirection,
                               [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                               [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                               string Status,
                               string ResultsVisibility,
                               Guid Creator,
                               Guid[] Admins,
                               int NbServers,
                               bool AutoScale,
                               int NbMaps,
                               int LeaderboardId,
                               string LeaderboardType,
                               int CompleteTimeout);
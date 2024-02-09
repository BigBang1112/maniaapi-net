using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CupOfTheDay(int Id,
                                 int Edition,
                                 Competition Competition,
                                 Challenge Challenge,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate);
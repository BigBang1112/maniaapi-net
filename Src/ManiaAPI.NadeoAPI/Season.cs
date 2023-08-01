using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record Season(string Uid,
                            string Name,
                            [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartTimestamp,
                            [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndTimestamp,
                            int RelativeStart,
                            int RelativeEnd,
                            int CampaignId,
                            bool Active);
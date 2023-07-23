using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record Maniapub(Guid CampaignUid,
                              string Name,
                              string AdType,
                              string ExternalUrl,
                              string Screen2x3Url,
                              string Screen16x9Url,
                              string Screen64x10Url,
                              string MediaUrl,
                              string DisplayFormat,
                              float Ratio,
                              float DisplayRatio,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndTimestamp,
                              int RelativeEnd);
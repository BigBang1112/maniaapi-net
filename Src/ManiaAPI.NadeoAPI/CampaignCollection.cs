using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CampaignCollection(Campaign[] CampaignList,
                                        [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset NextRequestTimestamp,
                                        int RelativeNextRequest);

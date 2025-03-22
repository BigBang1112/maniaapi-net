using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CampaignCollection(ImmutableArray<Campaign> CampaignList,
                                        [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset NextRequestTimestamp,
                                        int RelativeNextRequest,
                                        int ItemCount);

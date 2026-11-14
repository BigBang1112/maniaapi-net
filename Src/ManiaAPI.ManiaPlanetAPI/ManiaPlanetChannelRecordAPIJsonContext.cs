using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

// The Channel Record API documents numbers as strings ("score": "5000") but returns them as numbers, so both are read.
[JsonSerializable(typeof(ChannelPlayerRecordResponse))]
[JsonSerializable(typeof(ChannelLeaderboardResponse))]
[JsonSerializable(typeof(ChannelRecordsResponse))]
[JsonSerializable(typeof(ChannelLeaderboardRequest))]
[JsonSerializable(typeof(ChannelRecordsRequest))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal sealed partial class ManiaPlanetChannelRecordAPIJsonContext : JsonSerializerContext;

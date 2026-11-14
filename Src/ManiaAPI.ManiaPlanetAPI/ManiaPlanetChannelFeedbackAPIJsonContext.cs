using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

// Like the Channel Record API, the documentation shows numbers as strings, so both forms are read.
[JsonSerializable(typeof(ChannelFeedbackStatus))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal sealed partial class ManiaPlanetChannelFeedbackAPIJsonContext : JsonSerializerContext;

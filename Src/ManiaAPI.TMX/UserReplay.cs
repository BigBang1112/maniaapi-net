using ManiaAPI.TMX.Attributes;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.TMX;

[Fields]
public sealed record UserReplay(int ReplayId, [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 ReplayTime, int ReplayScore);
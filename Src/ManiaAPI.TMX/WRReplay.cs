using ManiaAPI.TMX.Attributes;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.TMX;

[Fields]
public sealed record WRReplay(User User, [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 ReplayTime, int ReplayScore, int ReplayId);
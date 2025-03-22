using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public sealed record WRReplay(User User, [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 ReplayTime, int ReplayScore, int ReplayId);
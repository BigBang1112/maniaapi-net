using System.Text.Json.Serialization;
using TmEssentials;
using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.Converters;

namespace ManiaAPI.TMX;

[Fields]
public record struct MapMedals(
    [property: JsonConverter(typeof(JsonBuggedTimeInt32Converter))] TimeInt32 Author,
    [property: JsonConverter(typeof(JsonBuggedTimeInt32Converter))] TimeInt32 Gold,
    [property: JsonConverter(typeof(JsonBuggedTimeInt32Converter))] TimeInt32 Silver,
    [property: JsonConverter(typeof(JsonBuggedTimeInt32Converter))] TimeInt32 Bronze);
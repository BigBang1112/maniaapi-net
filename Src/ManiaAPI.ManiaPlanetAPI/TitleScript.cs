using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record TitleScript([property: JsonPropertyName("filename")] string FileName,
                                 [property: JsonPropertyName("matchsettings")] string MatchSettings);
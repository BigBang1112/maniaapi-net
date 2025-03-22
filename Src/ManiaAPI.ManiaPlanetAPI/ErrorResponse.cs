using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record ErrorResponse(string? Error, string Message, [property: JsonPropertyName("error_description")] string? ErrorDescription);

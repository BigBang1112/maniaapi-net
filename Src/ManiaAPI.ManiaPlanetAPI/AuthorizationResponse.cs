using ManiaAPI.ManiaPlanetAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record AuthorizationResponse(
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in"), JsonConverter(typeof(TimeSpanSecondsConverter))] TimeSpan ExpiresIn,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token"), Obsolete("ManiaPlanet doesn't send a refresh token with client credentials, so this is always null. It will be removed in a future version.")] string? RefreshToken);
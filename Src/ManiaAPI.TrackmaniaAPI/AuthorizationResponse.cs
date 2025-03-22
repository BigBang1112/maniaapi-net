using ManiaAPI.TrackmaniaAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaAPI;

public sealed record AuthorizationResponse(
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in"), JsonConverter(typeof(TimeSpanSecondsConverter))] TimeSpan ExpiresIn,
    [property: JsonPropertyName("access_token")] string AccessToken);
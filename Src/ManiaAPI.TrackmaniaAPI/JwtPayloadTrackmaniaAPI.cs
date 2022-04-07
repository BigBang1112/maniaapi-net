using ManiaAPI.Base;
using ManiaAPI.Base.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaAPI;

public class JwtPayloadTrackmaniaAPI : JwtPayloadBase
{
    [JsonPropertyName("nbf")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset NotValidBefore { get; init; }

    public string[] Scopes { get; init; } = Array.Empty<string>();

    public static JwtPayloadTrackmaniaAPI DecodeFromAccessToken(string accessToken)
    {
        return JsonSerializer.Deserialize<JwtPayloadTrackmaniaAPI>(DecodeToJsonBytesFromAccessToken(accessToken), TrackmaniaAPI.JsonSerializerOptions)
            ?? throw new Exception("This should never happen");
    }
}

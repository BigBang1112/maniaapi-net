using ManiaAPI.Base;
using ManiaAPI.Base.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public class JwtPayloadNadeoAPI : JwtPayloadBase
{
    [JsonPropertyName("iss")]
    public string Issuer { get; init; } = "";

    [JsonPropertyName("rat")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset RefreshAt { get; init; }

    [JsonPropertyName("usg")]
    public string Usage { get; init; } = "";
    
    [JsonPropertyName("sid")]
    public Guid SessionId { get; init; }

    [JsonPropertyName("aun")]
    public string Account { get; init; } = "";
    
    public bool Rtk { get; init; }
    public bool Pce { get; init; }

    public static JwtPayloadNadeoAPI DecodeFromAccessToken(string accessToken)
    {
        return JsonSerializer.Deserialize<JwtPayloadNadeoAPI>(DecodeToJsonBytesFromAccessToken(accessToken), JsonApiBase.JsonSerializerOptions)
            ?? throw new Exception("This should never happen");
    }
}

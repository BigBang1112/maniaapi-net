using ManiaAPI.Base.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public class JwtPayload
{
    [JsonPropertyName("jti")]
    public string JwtId { get; init; } = "";

    [JsonPropertyName("iss")]
    public string Issuer { get; init; } = "";

    [JsonPropertyName("iat")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset IssuedAt { get; init; }

    [JsonPropertyName("rat")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset RefreshAt { get; init; }

    [JsonPropertyName("exp")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset ExpirationTime { get; init; }

    [JsonPropertyName("aud")]
    public string Audience { get; init; } = "";

    [JsonPropertyName("usg")]
    public string Usage { get; init; } = "";
    
    [JsonPropertyName("sid")]
    public Guid SessionId { get; init; }

    [JsonPropertyName("sub")]
    public Guid Subject { get; init; }

    [JsonPropertyName("aun")]
    public string Account { get; init; } = "";
    
    public bool Rtk { get; init; }
    public bool Pce { get; init; }

    public static JwtPayload DecodeFromAccessToken(string accessToken)
    {
        var jwtDot1Index = accessToken.IndexOf('.');
        var jwtDot2Index = accessToken.LastIndexOf('.');
        var jwtPayloadBase64UrlSafe = accessToken.Substring(jwtDot1Index + 1, jwtDot2Index - jwtDot1Index - 1);

        // Tried to be optimized but bruh I don't have energy for this xd
        var jwtPayloadBase64 = jwtPayloadBase64UrlSafe.Replace('_', '/').Replace('-', '+');

        switch (jwtPayloadBase64.Length % 4)
        {
            case 2: jwtPayloadBase64 += "=="; break;
            case 3: jwtPayloadBase64 += "="; break;
        }        

        var jwtPayloadDecoded = Convert.FromBase64String(jwtPayloadBase64);

        return JsonSerializer.Deserialize<JwtPayload>(jwtPayloadDecoded, NadeoAPI.JsonSerializerOptions) ?? throw new Exception("This should never happen");
    }
}

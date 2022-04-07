using ManiaAPI.Base.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.Base;

public class JwtPayloadBase
{
    [JsonPropertyName("jti")]
    public string JwtId { get; init; } = "";

    [JsonPropertyName("aud")]
    public string Audience { get; init; } = "";

    [JsonPropertyName("iat")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset IssuedAt { get; init; }

    [JsonPropertyName("exp")]
    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset ExpirationTime { get; init; }

    [JsonPropertyName("sub")]
    [JsonConverter(typeof(NullableGuidConverter))]
    public Guid? Subject { get; init; }

    protected static byte[] DecodeToJsonBytesFromAccessToken(string accessToken)
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

        return Convert.FromBase64String(jwtPayloadBase64);
    }
}

using ManiaAPI.TrackmaniaAPI.Converters;
using ManiaAPI.TrackmaniaAPI.JsonContexts;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaAPI;

public sealed record JwtPayloadTrackmaniaAPI(
    [property: JsonPropertyName("jti")] string JwtId,
    [property: JsonPropertyName("aud")] string Audience,
    [property: JsonPropertyName("iat"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset IssuedAt,
    [property: JsonPropertyName("nbf"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset NotBefore,
    [property: JsonPropertyName("exp"), JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset ExpirationTime,
    [property: JsonPropertyName("sub")] string Subject,
    [property: JsonPropertyName("scopes")] string[] Scopes)
{
    public static JwtPayloadTrackmaniaAPI DecodeFromAccessToken(string accessToken)
    {
        return JsonSerializer.Deserialize(DecodeToJsonBytesFromAccessToken(accessToken), TrackmaniaAPIJsonContext.Default.JwtPayloadTrackmaniaAPI)
            ?? throw new Exception("This should never happen");
    }

    private static byte[] DecodeToJsonBytesFromAccessToken(string accessToken)
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

        var data = Convert.FromBase64String(jwtPayloadBase64);

        Debug.WriteLine("Decoded JWT: " + Encoding.UTF8.GetString(data));

        return data;
    }
}

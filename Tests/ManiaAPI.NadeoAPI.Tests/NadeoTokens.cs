namespace ManiaAPI.NadeoAPI.Tests;

/// <summary>
/// Builds token responses in the shape returned by the Nadeo authentication endpoints.
/// </summary>
internal static class NadeoTokens
{
    public static string AccessToken(string audience, DateTimeOffset expiresAt, DateTimeOffset refreshAt, string account = "test-account")
    {
        return TestJwt.Create($$"""
            {
              "jti": "{{Guid.NewGuid()}}",
              "aud": "{{audience}}",
              "iat": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
              "exp": {{expiresAt.ToUnixTimeSeconds()}},
              "sub": "{{Guid.NewGuid()}}",
              "iss": "NadeoServices",
              "rat": {{refreshAt.ToUnixTimeSeconds()}},
              "usg": "Server",
              "sid": "{{Guid.NewGuid()}}",
              "aun": "{{account}}",
              "rtk": false,
              "pce": false
            }
            """);
    }

    public static string Response(string accessToken, string refreshToken = "refresh-token")
    {
        return $$"""{"accessToken":"{{accessToken}}","refreshToken":"{{refreshToken}}"}""";
    }

    /// <summary>
    /// A token response that stays valid for the whole test.
    /// </summary>
    public static HttpResponseMessage Valid(string audience, out string accessToken)
    {
        accessToken = AccessToken(audience, DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddMinutes(30));
        return FakeResponse.Json(Response(accessToken));
    }
}

using System.Text;

namespace ManiaAPI.Testing;

/// <summary>
/// Builds unsigned JWTs for tests that only need the payload to be decoded.
/// </summary>
internal static class TestJwt
{
    public static string Create(string payloadJson)
    {
        return $"{Base64Url("""{"alg":"HS256","typ":"JWT"}""")}.{Base64Url(payloadJson)}.signature";
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

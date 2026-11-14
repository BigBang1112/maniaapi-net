using System.Runtime.CompilerServices;

namespace ManiaAPI.ManiaPlanetAPI;

internal static class UrlHelper
{
    /// <summary>
    /// Escapes a value for use as a single URL path segment, so it can't add segments, a query, or a fragment to the URL.
    /// '@' is kept as-is since it's valid in a path segment (RFC 3986) and is part of every title UID.
    /// </summary>
    /// <exception cref="ArgumentException">The value is empty, <c>.</c>, or <c>..</c>, which would change the path instead of filling a segment.</exception>
    public static string EscapePathSegment(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);

        if (value is "." or "..")
        {
            throw new ArgumentException("The value can't be '.' or '..'.", paramName);
        }

        return Uri.EscapeDataString(value).Replace("%40", "@");
    }
}

using System.Runtime.CompilerServices;

namespace ManiaAPI.NadeoAPI;

internal static class UrlHelper
{
    /// <summary>
    /// Escapes a value for use as a single URL path segment, so it can't add segments, a query, or a fragment to the URL.
    /// </summary>
    /// <exception cref="ArgumentException">The value is empty, <c>.</c>, or <c>..</c>, which would change the path instead of filling a segment.</exception>
    public static string EscapePathSegment(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);

        if (value is "." or "..")
        {
            throw new ArgumentException("The value can't be '.' or '..'.", paramName);
        }

        return Uri.EscapeDataString(value);
    }
}

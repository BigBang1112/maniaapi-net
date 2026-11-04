namespace ManiaAPI.ManiaPlanetAPI;

internal static class UrlHelper
{
    /// <summary>
    /// Escapes a value for use as a single URL path segment. '@' is kept as-is since it's valid in a path segment (RFC 3986) and is part of every title UID.
    /// </summary>
    public static string EscapePathSegment(string value) => Uri.EscapeDataString(value).Replace("%40", "@");
}

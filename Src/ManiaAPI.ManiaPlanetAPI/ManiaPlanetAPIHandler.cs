using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed class ManiaPlanetAPIHandler
{
    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal string? ClientId { get; set; }
    internal string? ClientSecret { get; set; }
    internal ImmutableArray<string> Scopes { get; set; }

    internal JwtPayloadManiaPlanetAPI? Payload { get; set; }
}

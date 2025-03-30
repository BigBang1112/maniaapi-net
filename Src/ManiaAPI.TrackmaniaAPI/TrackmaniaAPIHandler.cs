using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace ManiaAPI.TrackmaniaAPI;

public sealed class TrackmaniaAPIHandler
{
    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal string? ClientId { get; set; }
    internal string? ClientSecret { get; set; }
    internal ImmutableArray<string> Scopes { get; set; }

    public JwtPayloadTrackmaniaAPI? Payload { get; internal set; }
}

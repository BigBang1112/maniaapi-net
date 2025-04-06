using System.Net.Http.Headers;

namespace ManiaAPI.TrackmaniaAPI;

public sealed class TrackmaniaAPIHandler
{
    internal AuthenticationHeaderValue? Authorization { get; set; }

    public TrackmaniaAPICredentials? Credentials { get; set; }

    public JwtPayloadTrackmaniaAPI? Payload { get; internal set; }
}

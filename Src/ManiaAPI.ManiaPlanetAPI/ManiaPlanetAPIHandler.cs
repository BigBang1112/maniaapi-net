using System.Net.Http.Headers;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed class ManiaPlanetAPIHandler
{
    public ManiaPlanetAPICredentials? Credentials { get; set; }

    internal AuthenticationHeaderValue? Authorization { get; set; }

    internal SemaphoreSlim AuthorizationLock { get; } = new(1, 1);

    internal JwtPayloadManiaPlanetAPI? Payload { get; set; }
}

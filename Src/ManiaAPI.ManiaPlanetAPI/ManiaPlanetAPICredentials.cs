using System.Collections.Immutable;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record ManiaPlanetAPICredentials(string ClientId, string ClientSecret, ImmutableArray<string> Scopes)
{
    public ManiaPlanetAPICredentials(string clientId, string clientSecret) : this(clientId, clientSecret, [])
    {
    }
}
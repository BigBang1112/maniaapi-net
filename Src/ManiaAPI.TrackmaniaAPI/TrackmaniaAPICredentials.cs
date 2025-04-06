using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI;

public sealed record TrackmaniaAPICredentials(string ClientId, string ClientSecret, ImmutableArray<string> Scopes)
{
    public TrackmaniaAPICredentials(string clientId, string clientSecret) : this(clientId, clientSecret, [])
    {
    }
}
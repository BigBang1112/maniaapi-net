using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record DedicatedServer(string Login,
    [property: JsonPropertyName("last_used_date")] DedicatedServerLastUsedDate LastUsedDate);

using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record DedicatedAccount(string Login,
    [property: JsonPropertyName("last_used_date")] DedicatedAccountLastUsedDate LastUsedDate);

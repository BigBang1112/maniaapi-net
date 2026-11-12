using System.Text.Json.Serialization;

namespace ManiaAPI.MCP.Accounts;

public sealed record LinkedTokens(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt, string[] Scopes);

[JsonSerializable(typeof(LinkedTokens))]
internal sealed partial class AccountJsonContext : JsonSerializerContext;

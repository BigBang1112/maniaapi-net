namespace ManiaAPI.MCP.Accounts;

public sealed record LinkedTokens(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt, string[] Scopes);

using System.Text.Json.Serialization;

namespace ManiaAPI.MCP.Accounts;

[JsonSerializable(typeof(LinkedTokens))]
internal sealed partial class AccountJsonContext : JsonSerializerContext;

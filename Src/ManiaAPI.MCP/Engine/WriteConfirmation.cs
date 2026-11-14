using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using Microsoft.AspNetCore.DataProtection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ManiaAPI.MCP.Engine;

public sealed class WriteConfirmation(IDataProtectionProvider protection, TimeProvider clock, McpOptions options)
{
    private readonly IDataProtector protector = protection.CreateProtector("ManiaAPI.MCP.WriteConfirmation.v1");

    public bool Authorize(
        Operation operation,
        JsonElement arguments,
        string scope,
        string caller,
        bool confirm,
        CallToolRequestParams request,
        McpServer? server)
    {
        if (operation.ReadOnly)
        {
            return true;
        }

        if (server?.IsMrtrSupported != true)
        {
            // The model sets confirm itself, so without the approval prompt nobody but the client approves the write
            if (!options.TrustClientApproval)
            {
                throw new ArgumentException("This client can't show the write approval prompt, so the write was refused. " +
                    "Use a client that supports MRTR, or set ManiaAPI:TrustClientApproval to true to rely on the client's own tool approval.");
            }

            if (!confirm)
            {
                throw new ArgumentException("This write requires confirm: true when the client cannot handle MRTR confirmation.");
            }

            return true;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join('\n', operation.Id, arguments.GetRawText(), scope, caller, request.Name))));

        if (request.RequestState is { } state)
        {
            string[] parts;

            try
            {
                parts = protector.Unprotect(state).Split('|');
            }
            catch (CryptographicException)
            {
                throw new ArgumentException("The write confirmation is invalid or belongs to another deployment.");
            }

            if (parts.Length != 2 || !long.TryParse(parts[0], CultureInfo.InvariantCulture, out var expires) ||
                expires < clock.GetUtcNow().ToUnixTimeSeconds() || parts[1] != hash)
            {
                throw new ArgumentException("The write confirmation expired or its arguments changed. Request confirmation again.");
            }

            var response = request.InputResponses?.TryGetValue("write_confirmation", out var input) == true
                ? input.Deserialize(InputResponse.ElicitResultJsonTypeInfo)
                : null;

            if (response?.Action == "accept" && response.Content?.TryGetValue("confirm", out var accepted) == true &&
                accepted.ValueKind == JsonValueKind.True)
            {
                return true;
            }

            throw new ArgumentException("The write was not approved.");
        }

        var expiration = clock.GetUtcNow().AddMinutes(5).ToUnixTimeSeconds();

        throw new InputRequiredException(new Dictionary<string, InputRequest>
        {
            ["write_confirmation"] = InputRequest.ForElicitation(new ElicitRequestParams
            {
                Message = $"Allow {operation.Id}? {operation.Summary} This changes data using the server's configured account.",
                RequestedSchema = new ElicitRequestParams.RequestSchema
                {
                    Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
                    {
                        ["confirm"] = new ElicitRequestParams.BooleanSchema { Description = "Approve this exact operation and its arguments." }
                    },
                    Required = ["confirm"]
                }
            })
        }, protector.Protect(expiration.ToString(CultureInfo.InvariantCulture) + "|" + hash));
    }
}

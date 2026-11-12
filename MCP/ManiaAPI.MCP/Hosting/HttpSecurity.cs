using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using ManiaAPI.MCP.Configuration;
using Microsoft.AspNetCore.RateLimiting;

namespace ManiaAPI.MCP.Hosting;

public static class HttpSecurity
{
    public const string ApiKeyHeader = "X-Api-Key";
    public const string RateLimitPolicy = "maniaapi";

    public static void Validate(McpOptions options, IEnumerable<string> addresses)
    {
        if (!options.Http.ApiKeys.Any(key => !string.IsNullOrWhiteSpace(key)))
        {
            throw new InvalidOperationException("HTTP hosting requires at least one non-empty ManiaApiMcp:Http:ApiKeys entry.");
        }

        var bindings = addresses.ToArray();

        if (options.AllowWrites && (bindings.Length == 0 || bindings.Any(address =>
            !Uri.TryCreate(address, UriKind.Absolute, out var uri) || !uri.IsLoopback)))
        {
            throw new InvalidOperationException("AllowWrites requires every HTTP listening address to be loopback.");
        }
    }

    public static bool Authenticate(string supplied, IReadOnlyList<byte[]> hashes, out int identity)
    {
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        identity = -1;

        for (var i = 0; i < hashes.Count; i++)
        {
            if (CryptographicOperations.FixedTimeEquals(suppliedHash, hashes[i]))
            {
                identity = i;
            }
        }

        return identity >= 0;
    }

    public static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Items["ManiaApiIdentity"] as string ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });
    }

    public static void UseApiKeys(WebApplication app, McpOptions options)
    {
        var hashes = options.Http.ApiKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => SHA256.HashData(Encoding.UTF8.GetBytes(key)))
            .ToArray();

        app.Use(async (context, next) =>
        {
            var accountManagement = context.Request.Path.StartsWithSegments("/account-links") &&
                (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method) ||
                 context.Request.Path == "/account-links/status");

            if (context.Request.Path.StartsWithSegments("/mcp") || context.Request.Path == "/readyz" || accountManagement)
            {
                var keys = context.Request.Headers[ApiKeyHeader];

                if (keys.Count != 1 || keys[0] is not { Length: > 0 } supplied ||
                    !Authenticate(supplied, hashes, out var identity))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                context.Items["ManiaApiIdentity"] = Convert.ToHexString(hashes[identity]);
            }

            await next(context);
        });
    }
}

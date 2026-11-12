using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using ModelContextProtocol.AspNetCore;
using ManiaAPI.MCP.Resources;
using ManiaAPI.MCP.Accounts;

namespace ManiaAPI.MCP;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var http = args.Contains("--http", StringComparer.Ordinal);
        var hostArgs = new List<string>();
        string[]? toolsets = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--http")
            {
                continue;
            }

            if (args[i] == "--toolsets")
            {
                if (++i >= args.Length)
                {
                    throw new ArgumentException("--toolsets requires a comma-separated list.");
                }

                toolsets = args[i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                continue;
            }

            hostArgs.Add(args[i]);
        }

        if (http)
        {
            var builder = WebApplication.CreateBuilder(hostArgs.ToArray());
            builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
            Configure(builder.Configuration, hostArgs.ToArray());

            builder.Services.AddManiaApiMcp(builder.Configuration, toolsets);
            HttpSecurity.AddRateLimiting(builder.Services);

            AddGateway(builder.Services)
                .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless);

            var app = builder.Build();
            var options = app.Services.GetRequiredService<McpOptions>();

            var addresses = builder.Configuration.GetSection("Kestrel:Endpoints").GetChildren()
                .Select(endpoint => endpoint["Url"])
                .Where(url => url is not null)
                .Select(url => url!)
                .Concat(app.Urls)
                .ToArray();

            HttpSecurity.Validate(options, addresses.Length == 0 ? ["http://localhost:5000"] : addresses);
            HttpSecurity.UseApiKeys(app, options);
            AccountLinking.Map(app, options);
            app.UseRateLimiter();

            app.MapGet("/healthz", () => Results.Text("healthy"));
            app.MapGet("/readyz", (UpstreamHealth health, IServiceProvider services) => Results.Text(
                health.Snapshot(services).ToJsonString(),
                "application/json",
                statusCode: health.Ready ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable));

            app.MapMcp("/mcp").RequireRateLimiting(HttpSecurity.RateLimitPolicy);

            foreach (var toolset in options.Toolsets.Distinct(StringComparer.Ordinal))
            {
                app.MapMcp("/mcp/" + toolset).RequireRateLimiting(HttpSecurity.RateLimitPolicy);
            }

            await app.RunAsync();
        }
        else
        {
            var builder = Host.CreateApplicationBuilder(hostArgs.ToArray());
            Configure(builder.Configuration, hostArgs.ToArray());

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
            builder.Services.AddManiaApiMcp(builder.Configuration, toolsets);

            if (builder.Configuration.GetValue<bool>("ManiaApiMcp:AccountLinking:Enabled"))
            {
                throw new InvalidOperationException("Player account linking requires the HTTP transport.");
            }

            AddGateway(builder.Services).WithStdioServerTransport();

            await builder.Build().RunAsync();
        }
    }

    private static void Configure(ConfigurationManager configuration, string[] args)
    {
        configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
        configuration.AddKeyPerFile("/run/secrets", optional: true);
        configuration.AddEnvironmentVariables();
        configuration.AddCommandLine(args);
    }

    private static Microsoft.Extensions.DependencyInjection.IMcpServerBuilder AddGateway(IServiceCollection services)
    {
        return services.AddMcpServer()
            .WithListToolsHandler((context, _) =>
                ValueTask.FromResult(context.Services!.GetRequiredService<GatewayTools>().List()))
            .WithCallToolHandler((context, cancellationToken) =>
                context.Services!.GetRequiredService<GatewayTools>().CallAsync(context.Params!, context.Services!, cancellationToken, context.Server))
            .WithListResourceTemplatesHandler((context, _) =>
                ValueTask.FromResult(context.Services!.GetRequiredService<ResourceService>().List()))
            .WithReadResourceHandler((context, cancellationToken) =>
                context.Services!.GetRequiredService<ResourceService>().ReadAsync(context.Params!.Uri, context.Services!, cancellationToken));
    }
}

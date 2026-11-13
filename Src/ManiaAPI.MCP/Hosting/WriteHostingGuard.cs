using ManiaAPI.MCP.Configuration;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace ManiaAPI.MCP.Hosting;

public sealed class WriteHostingGuard(IServiceProvider services, McpOptions options) : IHostedLifecycleService
{
    public bool Validated { get; private set; }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        if (options.AllowWrites && services.GetService<IServer>() is { } server)
        {
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
            HttpSecurity.Validate(options, addresses);
        }

        Validated = true;

        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

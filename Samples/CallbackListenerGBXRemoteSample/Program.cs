using ManiaAPI.GBXRemote;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<GBXRemoteClient>();

using var client = await GBXRemoteClient.ConnectAsync("127.0.0.1", logger: logger);

await client.CallAsync("Authenticate", "SuperAdmin", "SuperAdmin");
await client.CallAsync("EnableCallbacks", true);

await foreach (var callback in client.StreamCallbacksAsync())
{
    logger.LogInformation("{MethodName}: {MethodParams}", callback.MethodName, string.Join(", ", callback.MethodParams));
}
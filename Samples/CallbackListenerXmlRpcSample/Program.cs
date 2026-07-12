using ManiaAPI.XmlRpc;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<XmlRpcClient>();

using var xmlRpc = await XmlRpcClient.ConnectAsync("127.0.0.1", logger: logger);

await xmlRpc.CallAsync("Authenticate", "SuperAdmin", "SuperAdmin");
await xmlRpc.CallAsync("EnableCallbacks", true);

await foreach (var callback in xmlRpc.StreamCallbacksAsync())
{
    logger.LogInformation("{MethodName}: {MethodParams}", callback.MethodName, string.Join(", ", callback.MethodParams));
}
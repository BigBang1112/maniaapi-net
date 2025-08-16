using ManiaAPI.XmlRpc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

var logger = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Trace);
}).CreateLogger<XmlRpcClient>();

using var xmlRpc = await XmlRpcClient.ConnectAsync("127.0.0.1", logger: logger);

object?[] authenticationResult = await xmlRpc.CallAsync("Authenticate", ["SuperAdmin", "SuperAdmin"]);

if (authenticationResult is not [true])
{
    throw new Exception("Authentication failed.");
}

var networkStats = await xmlRpc.CallAsync("GetNetworkStats");

Console.WriteLine(JsonConvert.SerializeObject(networkStats, Formatting.Indented));
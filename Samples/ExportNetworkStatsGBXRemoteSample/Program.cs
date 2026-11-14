using ManiaAPI.GBXRemote;
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
}).CreateLogger<GBXRemoteClient>();

using var client = await GBXRemoteClient.ConnectAsync("127.0.0.1", logger: logger);

// Wrong credentials throw GBXRemoteFaultException
await client.CallAsync("Authenticate", ["SuperAdmin", "SuperAdmin"]);

var networkStats = await client.CallAsync("GetNetworkStats");

Console.WriteLine(JsonConvert.SerializeObject(networkStats, Formatting.Indented));
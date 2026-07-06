# ManiaAPI.XmlRpc

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.XmlRpc?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.XmlRpc/)

Integrates the GBXRemote (XML-RPC) communication used between controllers and servers.

Both versions (`GBXRemote 2` and `GBXRemote 1`) are supported.

This solution tries to be lightweight and compatible with as many Nadeo games as possible. For a better strongly-typed GBXRemote, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

**This package is still experimental, the API can change drastically.**

## Usage

```cs
using ManiaAPI.XmlRpc;

await using var client = await XmlRpcClient.ConnectAsync("127.0.0.1");

object? authenticationResult = await client.CallAsync("Authenticate", ["SuperAdmin", "SuperAdmin"]);

if (authenticationResult is not true)
{
    throw new Exception("Authentication failed.");
}

object? gameDataResult = await client.CallAsync("GameDataDirectory");

if (gameDataResult is not string gameDataDirectory)
{
    throw new Exception("Failed to retrieve game data directory.");
}

Console.WriteLine($"Game data directory: {gameDataDirectory}");
```

For callbacks:

```cs
object? enableCallbacksResult = await client.CallAsync("EnableCallbacks", true);

if (enableCallbacksResult is not true)
{
    throw new Exception("Failed to enable callbacks.");
}

// Subscribe to callbacks
client.Callback += async (methodName, methodParams, cancellationToken) =>
{
    Console.WriteLine($"{methodName}: {string.Join(", ", methodParams)}");
};

client.On("TrackMania.PlayerConnect", async (methodParams, cancellationToken) => 
{
    // ...
});

// Keep the connection until the server closes it
await xmlRpc.WaitForCloseAsync();
```
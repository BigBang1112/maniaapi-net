# ManiaAPI.GBXRemote

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.GBXRemote?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.GBXRemote/)

Integrates the GBXRemote (XML-RPC) communication used between controllers and servers.

Both versions (`GBXRemote 2` and `GBXRemote 1`) are supported.

This solution tries to be lightweight and compatible with as many Nadeo games as possible. For a better strongly-typed GBXRemote, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

**This package is still experimental, the API can change drastically.**

## Migration from ManiaAPI.XmlRpc

Replace the package reference with `ManiaAPI.GBXRemote` and use the `ManiaAPI.GBXRemote` namespace. Types use the `GBXRemote` prefix in place of `XmlRpc`:

| Previous name | New name |
| --- | --- |
| `XmlRpcClient` | `GBXRemoteClient` |
| `XmlRpcCall` | `GBXRemoteCall` |
| `XmlRpcCallResult` | `GBXRemoteCallResult` |
| `XmlRpcCallback` | `GBXRemoteCallback` |
| `XmlRpcCallbackMessage` | `GBXRemoteCallbackMessage` |

Method names and the wire protocol are unchanged.

`ManiaAPI.XmlRpc.XmlRpcClient` remains available as an obsolete compatibility class that inherits from `GBXRemoteClient`. Its `ConnectAsync` overloads return the compatibility type; new code should use `GBXRemoteClient`.

## Usage

```cs
using ManiaAPI.GBXRemote;

await using var client = await GBXRemoteClient.ConnectAsync("127.0.0.1");

object authenticationResult = await client.CallAsync("Authenticate", ["SuperAdmin", "SuperAdmin"]);

if (authenticationResult is not true)
{
    throw new Exception("Authentication failed.");
}

string gameDataDirectory = await client.CallAsync<string>("GameDataDirectory");

Console.WriteLine($"Game data directory: {gameDataDirectory}");
```

For callbacks:

```cs
bool enableCallbacksResult = await client.CallAsync<bool>("EnableCallbacks", true);

if (!enableCallbacksResult)
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
await client.WaitForCloseAsync();
```

## Resilience

`GBXRemoteClient` communicates over a raw TCP connection. Add [`Microsoft.Extensions.Resilience`](https://www.nuget.org/packages/Microsoft.Extensions.Resilience) and wrap `ConnectAsync` in a retry pipeline to handle transient connection failures:

```cs
using ManiaAPI.GBXRemote;
using Polly;
using Polly.Retry;

var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Exponential
    })
    .Build();

await using var client = await pipeline.ExecuteAsync(async token =>
    await GBXRemoteClient.ConnectAsync("127.0.0.1", 5000, cancellationToken: token));
```

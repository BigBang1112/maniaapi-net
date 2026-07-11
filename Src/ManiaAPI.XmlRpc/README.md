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

`XmlRpcClient` communicates over a raw TCP connection. Wrap `ConnectAsync` with a [`Polly`](https://www.nuget.org/packages/Polly) pipeline to retry on transient connection failures:

```cs
using ManiaAPI.XmlRpc;
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
    await XmlRpcClient.ConnectAsync("127.0.0.1", 5000, cancellationToken: token));
```
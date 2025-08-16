# ManiaAPI.XmlRpc

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.XmlRpc?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.XmlRpc/)

Integrates the XML-RPC communication used between controllers and servers.

This solution tries to be lightweight and compatible with as many Nadeo games as possible. For a better strongly-typed XML-RPC, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

**This package is still experimental, the API can change drastically.**

## Usage

```cs
using ManiaAPI.XmlRpc;

using var xmlRpc = await XmlRpcClient.ConnectAsync("127.0.0.1");

object?[] authenticationResult = await xmlRpc.CallAsync("Authenticate", ["SuperAdmin", "SuperAdmin"]);

if (authenticationResult is not [true])
{
    throw new Exception("Authentication failed.");
}

object?[] result = await xmlRpc.CallAsync("GameDataDirectory");
```

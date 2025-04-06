# ManiaAPI.TrackmaniaWS

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaWS?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaWS/)

Wraps https://ws.trackmania.com/ (old TMF web API). **This API requires authorization (via constructor).**

## Features

- Get player info (registration ID from login for example)

More will be added in the future.

## Setup

```cs
using ManiaAPI.TrackmaniaWS;

var ws = new TrackmaniaWS("tmf_yourapp", "password");

// Ready to use
```

For DI, consider using the `ManiaAPI.TrackmaniaWS.Extensions.Hosting` package.
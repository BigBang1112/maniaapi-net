# ManiaAPI.TrackmaniaIO

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaIO?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaIO/)

Wraps the https://trackmania.io/ API which provides services around the Nadeo API.

This API is more moderated and cached, but doesn't require you to authenticate with it.

## Features

- Get various campaigns (including weekly shorts)
- Get leaderboards
- Get recent world records
- Get map info
- Get clubs, including their members and activities
- Get club rooms
- Get track of the days
- Get advertisements
- Get competitions

## Setup

```cs
using ManiaAPI.TrackmaniaIO;

var tmio = new TrackmaniaIO("Example from ManiaAPI.NET"); // User agent to comply with https://openplanet.dev/tmio/api
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.TrackmaniaIO;

builder.Services.AddHttpClient<TrackmaniaIO>();
```
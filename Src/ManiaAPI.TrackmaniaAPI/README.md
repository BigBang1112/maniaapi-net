# ManiaAPI.TrackmaniaAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaAPI/)

Wraps https://api.trackmania.com/doc (Trackmania web API). **This API requires authorization.**

## Features

- Get display names
- Get account IDs from display names
- Get user's map records

More will be added in the future.

## Setup

For the list of scopes, see [the API docs](https://api.trackmania.com/doc). Generate your credentials [here](https://api.trackmania.com/manager).

```cs
using ManiaAPI.TrackmaniaAPI;

var tm = new TrackmaniaAPI();

await tm.AuthorizeAsync("clientId", "clientSecret", ["clubs", "read_favorite"]);

// Ready to use
```

For DI, consider using the `ManiaAPI.TrackmaniaAPI.Extensions.Hosting` package.
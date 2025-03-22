# ManiaAPI.ManiaPlanetAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI/)

Wraps https://maniaplanet.com/swagger (ManiaPlanet web API). This API does not require authorization, but you can authorize to have more methods available.

## Possibilities

[All available on Swagger.](https://maniaplanet.com/swagger)

## Setup

For the list of scopes, see [here at the bottom](https://doc.maniaplanet.com/web-services/oauth2). Generate your credentials [here](https://maniaplanet.com/web-services-manager).

```cs
using ManiaAPI.ManiaPlanetAPI;

var mp = new ManiaPlanetAPI();

// You can optionally authorize to do more things, and possibly be less limited
await mp.AuthorizeAsync("clientId", "clientSecret", ["basic", "dedicated", "maps"]);

// Ready to use
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.ManiaPlanetAPI;

builder.Services.AddScoped<ManiaPlanetAPI>();
builder.Services.AddHttpClient<ManiaPlanetAPI>();
```
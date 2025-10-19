# ManiaAPI.ManiaPlanetAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI/)

Wraps https://maniaplanet.com/swagger (ManiaPlanet web API). This API does not require authorization, but you can authorize to have more methods available.

## Features

- [All available on Swagger](https://maniaplanet.com/swagger)
- Couple of ingame requests:
  - Authenticating a ManiaPlanet user via login and token
  - Downloading a title pack
  - Get title pack info (contains more info than from WebServices)

## Setup

For the list of scopes, see [here at the bottom](https://doc.maniaplanet.com/web-services/oauth2). Generate your credentials [here](https://maniaplanet.com/web-services-manager).

```cs
using ManiaAPI.ManiaPlanetAPI;

var mp = new ManiaPlanetAPI();

// You can optionally authorize to do more things, and possibly be less limited
await mp.AuthorizeAsync("clientId", "clientSecret", ["basic", "dedicated", "maps"]);

// Ready to use
```

For ingame API, use the `ManiaPlanetIngameAPI`. This is not an authenticated API, but you can use it to authenticate logins of users or servers by their token.

```cs
using ManiaAPI.ManiaPlanetAPI;

var mpIngame = new ManiaPlanetIngameAPI();

// Authenticate a user
var user = await mpIngame.AuthenticateAsync("username", "token");

if (user.Login != "username")
{
	throw new Exception("Invalid token");
}
```

For DI, consider using the `ManiaAPI.ManiaPlanetAPI.Extensions.Hosting` package, but for `ManiaPlanetIngameAPI`, you can just directly call:

```cs
using ManiaAPI.ManiaPlanetAPI;

builder.Services.AddHttpClient<ManiaPlanetIngameAPI>();
```
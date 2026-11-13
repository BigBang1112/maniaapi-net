# ManiaAPI.ManiaPlanetAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI/)

Wraps https://maniaplanet.com/swagger (ManiaPlanet web API). This API does not require authorization, but you can authorize to have more methods available.

## Features

- [All available on Swagger](https://maniaplanet.com/swagger)
- Couple of ingame requests:
  - Authenticating a ManiaPlanet user via login and token
  - Search title packs (contains more info than from WebServices)
  - Get title pack info (contains more info than from WebServices)
  - Search online servers (if you have login and token, allows filtering by favorites)
  - Downloading a title pack
- Channel map leaderboards from the [Channel Record API](https://records.prod.live.maniaplanet.com/):
  - Top 10 records with the ranks of chosen players
  - Pages of records, up to 101 at once
  - A player's record and rank
- Channel map feedback totals (yes/no votes and star ratings) from the [Channel Feedback API](https://feedback.prod.live.maniaplanet.com/)

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

For leaderboards of channel maps, use the `ManiaPlanetChannelRecordAPI`. It doesn't need authorization.

```cs
using ManiaAPI.ManiaPlanetAPI;

var channel = new ManiaPlanetChannelRecordAPI();

// Top 10, the number of records, and the ranks of the given players
var leaderboard = await channel.GetLeaderboardAsync("9W_9qjV56i67CWpYDclFyOhz5g", ["lbt76240"]);

// Records 101 to 200
var page = await channel.GetRecordsAsync("9W_9qjV56i67CWpYDclFyOhz5g", offset: 100, length: 100);

// A player's record, or null Record when they have none
var player = await channel.GetPlayerRecordAsync("9W_9qjV56i67CWpYDclFyOhz5g", "lbt76240");
```

For feedback of channel maps, use the `ManiaPlanetChannelFeedbackAPI`. Only the totals are public; votes and ratings of single maps need the game servers' credentials.

```cs
using ManiaAPI.ManiaPlanetAPI;

var feedback = new ManiaPlanetChannelFeedbackAPI();

var status = await feedback.GetStatusAsync();

Console.WriteLine($"{status.YesNo.VotesYes} yes, {status.YesNo.VotesNo} no, {status.Stars.Notes5Star} five-star ratings");
```

For DI, consider using the `ManiaAPI.ManiaPlanetAPI.Extensions.Hosting` package.
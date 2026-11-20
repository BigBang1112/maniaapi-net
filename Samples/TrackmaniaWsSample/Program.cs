using ManiaAPI.TrackmaniaWS;

var tmws = new TrackmaniaWS("login", "password", "MyApp/1.0 (contact: me@example.com)");

var player = await tmws.GetPlayerAsync("bigbang1112");

Console.WriteLine();
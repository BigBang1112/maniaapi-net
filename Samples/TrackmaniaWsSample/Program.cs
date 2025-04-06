using ManiaAPI.TrackmaniaWS;

var tmws = new TrackmaniaWS("login", "password");

var player = await tmws.GetPlayerAsync("bigbang1112");

Console.WriteLine();
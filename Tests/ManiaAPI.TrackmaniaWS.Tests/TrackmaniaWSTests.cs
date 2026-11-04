using System.Text;

namespace ManiaAPI.TrackmaniaWS.Tests;

public class TrackmaniaWSTests
{
    private const string PlayerJson = """
        {
          "id": "123",
          "login": "bigbang1112",
          "nickname": "$f00Big",
          "united": "1",
          "path": "World|Europe|Czech republic",
          "idZone": "5"
        }
        """;

    [Test]
    public async Task GetPlayerAsync_SendsBasicAuthAndParsesStringValues()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson);
        using var ws = new TrackmaniaWS("api-user", "api-password", fake.CreateClient(TrackmaniaWS.BaseAddress));

        var player = await ws.GetPlayerAsync("bigbang1112");

        await Assert.That(fake.Request.Url).IsEqualTo("http://ws.trackmania.com/tmf/players/bigbang1112/");
        await Assert.That(fake.Request.Authorization?.Scheme).IsEqualTo("Basic");
        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes("api-user:api-password")));
        await Assert.That(player).IsEqualTo(new Player(123, "bigbang1112", "$f00Big", United: true, "World|Europe|Czech republic", IdZone: 5));
    }

    [Test]
    public async Task GetPlayerAsync_WithCredentialsRecord_SendsSameAuthorization()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson);
        using var ws = new TrackmaniaWS(new TrackmaniaWSCredentials("api-user", "api-password"), fake.CreateClient(TrackmaniaWS.BaseAddress));

        await ws.GetPlayerAsync("bigbang1112");

        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes("api-user:api-password")));
    }

    [Test]
    [Arguments("0", false)]
    [Arguments("", false)]
    [Arguments("1", true)]
    public async Task GetPlayerAsync_UnitedIsTrueOnlyForOne(string united, bool expected)
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson.Replace("\"united\": \"1\"", $"\"united\": \"{united}\""));
        using var ws = new TrackmaniaWS("api-user", "api-password", fake.CreateClient(TrackmaniaWS.BaseAddress));

        var player = await ws.GetPlayerAsync("bigbang1112");

        await Assert.That(player.United).IsEqualTo(expected);
    }

    [Test]
    public async Task GetPlayerAsync_Unauthorized_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ws = new TrackmaniaWS("api-user", "wrong", fake.CreateClient(TrackmaniaWS.BaseAddress));

        await Assert.ThrowsAsync<HttpRequestException>(() => ws.GetPlayerAsync("bigbang1112"));
    }
}

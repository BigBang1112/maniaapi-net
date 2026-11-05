using System.Text;

namespace ManiaAPI.TrackmaniaWS.Tests;

public class TrackmaniaWSTests
{
    private const string ApiUser = "api-user";
    private const string ApiPassword = "api-password";
    private const string Login = "bigbang1112";

    private static readonly string ExpectedBasicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ApiUser}:{ApiPassword}"));

    [Test]
    public async Task GetPlayerAsync_RequestsPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Url).IsEqualTo($"http://ws.trackmania.com/tmf/players/{Login}/");
    }

    [Test]
    public async Task GetPlayerAsync_SendsBasicAuth()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Authorization?.Scheme).IsEqualTo("Basic");
        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(ExpectedBasicCredentials);
    }

    [Test]
    public async Task GetPlayerAsync_WithCredentialsRecord_SendsSameBasicAuth()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = new TrackmaniaWS(new TrackmaniaWSCredentials(ApiUser, ApiPassword), fake.CreateClient(TrackmaniaWS.BaseAddress));

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(ExpectedBasicCredentials);
    }

    [Test]
    public async Task GetPlayerAsync_StringValues_ReturnsParsedPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        var player = await ws.GetPlayerAsync(Login);

        await Assert.That(player).IsEqualTo(new Player(123, Login, "$f00Big", United: true, "World|Europe|Czech republic", IdZone: 5));
    }

    [Test]
    [Arguments("0", false)]
    [Arguments("", false)]
    [Arguments("1", true)]
    public async Task GetPlayerAsync_UnitedIsTrueOnlyForOne(string united, bool expected)
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson(united));
        using var ws = CreateWs(fake);

        var player = await ws.GetPlayerAsync(Login);

        await Assert.That(player.United).IsEqualTo(expected);
    }

    [Test]
    public async Task GetPlayerAsync_Unauthorized_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ws = CreateWs(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => ws.GetPlayerAsync(Login));
    }

    private static TrackmaniaWS CreateWs(FakeHttpMessageHandler fake) => new(ApiUser, ApiPassword, fake.CreateClient(TrackmaniaWS.BaseAddress));

    /// <summary>
    /// A player as the web services return it, with every value as a string.
    /// </summary>
    private static string PlayerJson(string united = "1") => $$"""
        {
          "id": "123",
          "login": "{{Login}}",
          "nickname": "$f00Big",
          "united": "{{united}}",
          "path": "World|Europe|Czech republic",
          "idZone": "5"
        }
        """;
}

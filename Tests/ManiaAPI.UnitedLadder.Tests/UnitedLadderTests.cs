namespace ManiaAPI.UnitedLadder.Tests;

public class UnitedLadderTests
{
    private const string Login = "bigbang1112";

    private static readonly string PlayerJson = $$"""
        {
          "id": 123,
          "login": "{{Login}}",
          "nickname": "$f00Big",
          "united": true,
          "path": "World|Europe|Czech republic",
          "idZone": 5,
          "legacy": false,
          "tags": [
            { "url": "https://example.com/tag.png", "id": "tag", "imageName": "tag.png", "comment": "Nice", "deliverer": "admin" }
          ]
        }
        """;

    [Test]
    public async Task GetPlayerAsync_RequestsPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson);
        using var ul = new UnitedLadder(fake.CreateClient());

        await ul.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Url).IsEqualTo($"https://api.ul.unitedascenders.xyz/player/{Login}");
    }

    [Test]
    public async Task GetPlayerAsync_ReturnsParsedPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson);
        using var ul = new UnitedLadder(fake.CreateClient());

        var player = await ul.GetPlayerAsync(Login);

        await Assert.That(player.Id).IsEqualTo(123);
        await Assert.That(player.United).IsTrue();
        await Assert.That(player.IdZone).IsEqualTo(5);
        await Assert.That(player.Legacy).IsFalse();
        await Assert.That(player.Tags).IsEquivalentTo([new Tag("https://example.com/tag.png", "tag", "tag.png", "Nice", "admin")]);
    }

    [Test]
    public async Task GetPlayerAsync_NullTags_ReturnsNullTags()
    {
        var fake = FakeHttpMessageHandler.Json("""{"id":1,"login":"a","nickname":"a","united":false,"path":"World","idZone":1,"legacy":true,"tags":null}""");
        using var ul = new UnitedLadder(fake.CreateClient());

        var player = await ul.GetPlayerAsync("a");

        await Assert.That(player.Tags).IsNull();
    }

    [Test]
    public async Task GetPlayerAsync_NotFound_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.NotFound);
        using var ul = new UnitedLadder(fake.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(() => ul.GetPlayerAsync("nobody"));
    }
}

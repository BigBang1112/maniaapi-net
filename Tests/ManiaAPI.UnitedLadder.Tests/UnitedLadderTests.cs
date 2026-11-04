namespace ManiaAPI.UnitedLadder.Tests;

public class UnitedLadderTests
{
    [Test]
    public async Task GetPlayerAsync_RequestsPlayerAndParsesTags()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {
              "id": 123,
              "login": "bigbang1112",
              "nickname": "$f00Big",
              "united": true,
              "path": "World|Europe|Czech republic",
              "idZone": 5,
              "legacy": false,
              "tags": [
                { "url": "https://example.com/tag.png", "id": "tag", "imageName": "tag.png", "comment": "Nice", "deliverer": "admin" }
              ]
            }
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var player = await ul.GetPlayerAsync("bigbang1112");

        await Assert.That(fake.Request.Url).IsEqualTo("https://api.ul.unitedascenders.xyz/player/bigbang1112");
        await Assert.That(player.Id).IsEqualTo(123);
        await Assert.That(player.United).IsTrue();
        await Assert.That(player.IdZone).IsEqualTo(5);
        await Assert.That(player.Tags).IsNotNull();
        await Assert.That(player.Tags![0]).IsEqualTo(new Tag("https://example.com/tag.png", "tag", "tag.png", "Nice", "admin"));
    }

    [Test]
    public async Task GetPlayerAsync_WithoutTags_ParsesNull()
    {
        var fake = FakeHttpMessageHandler.Json("""{"id":1,"login":"a","nickname":"a","united":false,"path":"World","idZone":1,"legacy":true,"tags":null}""");
        using var ul = new UnitedLadder(fake.CreateClient());

        var player = await ul.GetPlayerAsync("a");

        await Assert.That(player.Tags).IsNull();
        await Assert.That(player.Legacy).IsTrue();
    }

    [Test]
    public async Task GetPlayerAsync_NotFound_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.NotFound);
        using var ul = new UnitedLadder(fake.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(() => ul.GetPlayerAsync("nobody"));
    }
}

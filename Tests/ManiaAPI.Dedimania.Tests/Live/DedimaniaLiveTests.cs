namespace ManiaAPI.Dedimania.Tests.Live;

[LiveTest]
public class DedimaniaLiveTests
{
    // A01-Race, the first official TMNF map, with records of both Rounds and Time Attack on both servers.
    private const string Uid = "BeySZdnfuSh4nHY5xztiXLmlrXe";

    [Test]
    [Arguments(DedimaniaGame.TMF)]
    [Arguments(DedimaniaGame.ManiaPlanet)]
    public async Task GetMapAsync_ReturnsMapWithRecords(DedimaniaGame game)
    {
        using var dedimania = new Dedimania(game);

        var map = await dedimania.GetMapAsync(Uid);

        await Assert.That(map).IsNotNull();
        await Assert.That(map!.Uid).IsEqualTo(Uid);
        await Assert.That(map.Name).IsEqualTo("A01-Race");
        await Assert.That(map.Environment).IsEqualTo("Stadium");
        await Assert.That(map.AuthorLogin).IsEqualTo("Nadeo");
        await Assert.That(map.Records).IsNotEmpty();
        await Assert.That(map.Records.All(r => r.Timestamp is not null)).IsTrue();
    }

    [Test]
    [Arguments(DedimaniaGame.TMF, GameMode.Rounds)]
    [Arguments(DedimaniaGame.TMF, GameMode.TimeAttack)]
    [Arguments(DedimaniaGame.ManiaPlanet, GameMode.Rounds)]
    [Arguments(DedimaniaGame.ManiaPlanet, GameMode.TimeAttack)]
    public async Task GetMapAsync_Mode_ReturnsRecordsOfMode(DedimaniaGame game, GameMode mode)
    {
        using var dedimania = new Dedimania(game);

        var map = await dedimania.GetMapAsync(Uid, mode);

        await Assert.That(map).IsNotNull();
        await Assert.That(map!.Records).IsNotEmpty();
        await Assert.That(map.Records.All(r => r.Mode == mode)).IsTrue();
    }

    [Test]
    [Arguments(DedimaniaGame.TMF)]
    [Arguments(DedimaniaGame.ManiaPlanet)]
    public async Task GetMapAsync_UnknownMap_ReturnsNull(DedimaniaGame game)
    {
        using var dedimania = new Dedimania(game);

        var map = await dedimania.GetMapAsync($"nomap{Guid.NewGuid():N}");

        await Assert.That(map).IsNull();
    }
}

namespace ManiaAPI.Dedimania.Tests.Live;

[LiveTest]
public class DedimaniaLiveTests
{
    // A01-Race, the first official TMNF map, with records of both Rounds and Time Attack.
    private const string Uid = "BeySZdnfuSh4nHY5xztiXLmlrXe";

    [Test]
    public async Task GetMapAsync_ReturnsMapWithRecords()
    {
        using var dedimania = new Dedimania();

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
    [Arguments(GameMode.Rounds)]
    [Arguments(GameMode.TimeAttack)]
    public async Task GetMapAsync_Mode_ReturnsRecordsOfMode(GameMode mode)
    {
        using var dedimania = new Dedimania();

        var map = await dedimania.GetMapAsync(Uid, mode);

        await Assert.That(map).IsNotNull();
        await Assert.That(map!.Records).IsNotEmpty();
        await Assert.That(map.Records.All(r => r.Mode == mode)).IsTrue();
    }

    [Test]
    public async Task GetMapAsync_UnknownMap_ReturnsNull()
    {
        using var dedimania = new Dedimania();

        var map = await dedimania.GetMapAsync($"nomap{Guid.NewGuid():N}");

        await Assert.That(map).IsNull();
    }
}

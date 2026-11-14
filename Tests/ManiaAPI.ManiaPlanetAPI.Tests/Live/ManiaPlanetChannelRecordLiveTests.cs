namespace ManiaAPI.ManiaPlanetAPI.Tests.Live;

/// <summary>
/// Live tests of the Channel Record API, which needs no credentials.
/// </summary>
[LiveTest]
[NotInParallel("ManiaPlanetChannelRecord")]
public class ManiaPlanetChannelRecordLiveTests
{
    /// <summary>
    /// The channel map used in the Channel Record API documentation, with over a thousand records.
    /// </summary>
    private const string MapUid = "9W_9qjV56i67CWpYDclFyOhz5g";

    [Test]
    public async Task Leaderboard_TopRangeAndPlayer_Agree()
    {
        var api = new ManiaPlanetChannelRecordAPI();

        var records = await api.GetRecordsAsync(MapUid, offset: 0, length: ManiaPlanetChannelRecordAPI.MaxLength);
        var best = records[0];

        var leaderboard = await api.GetLeaderboardAsync(MapUid, [best.UserLogin, "player-without-a-record"]);
        var player = await api.GetPlayerRecordAsync(MapUid, best.UserLogin);

        await Assert.That(records.Count).IsEqualTo(ManiaPlanetChannelRecordAPI.MaxLength);
        await Assert.That(records.Select(x => x.Rank)).IsEquivalentTo(Enumerable.Range(1, ManiaPlanetChannelRecordAPI.MaxLength));
        await Assert.That(leaderboard!.Top).IsEquivalentTo(records.Take(10));
        await Assert.That(leaderboard.PlayerRanks).IsEquivalentTo(new Dictionary<string, int> { [best.UserLogin] = 1 });
        await Assert.That(player!.Record).IsEqualTo(best);
        await Assert.That(player.RecordCount).IsEqualTo(leaderboard.RecordCount);
    }

    [Test]
    public async Task UnknownMap_HasNoLeaderboard()
    {
        var api = new ManiaPlanetChannelRecordAPI();

        await Assert.That(await api.GetPlayerRecordAsync("NoSuchMapUid000000000000000", "tona.")).IsNull();
        await Assert.That(await api.GetLeaderboardAsync("NoSuchMapUid000000000000000")).IsNull();
        await Assert.That(await api.GetRecordsAsync("NoSuchMapUid000000000000000")).IsEmpty();
    }
}

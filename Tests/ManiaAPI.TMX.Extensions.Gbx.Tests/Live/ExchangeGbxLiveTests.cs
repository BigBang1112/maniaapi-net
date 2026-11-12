using GBX.NET.Engines.Game;

namespace ManiaAPI.TMX.Extensions.Gbx.Tests.Live;

/// <summary>
/// Downloads files from the exchanges and reads them with GBX.NET. Only the headers are read: they hold what is checked here,
/// and reading a body would need an LZO implementation registered with GBX.NET.
/// </summary>
[LiveTest]
[NotInParallel("Exchange")]
public class ExchangeGbxLiveTests
{
    /// <summary>
    /// A popular TMNF track with many replays, the same one as in the TMX live tests.
    /// </summary>
    private const long TmnfTrackId = 2233;

    /// <summary>
    /// A popular Trackmania map with many replays, the same one as in the MX live tests.
    /// </summary>
    private const long TrackmaniaMapId = 120839;

    [Test]
    public async Task TMX_TrackAndReplayHeaders_MatchTheTrack()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var track = (await tmx.SearchTracksAsync(new TMX.SearchTracksParameters { Id = [TmnfTrackId] })).Results.Single();
        var replay = (await tmx.GetReplaysAsync(new TMX.GetReplaysParameters { TrackId = TmnfTrackId })).Results[0];

        var map = await tmx.GetTrackGbxHeaderNodeAsync(TmnfTrackId);
        var mapGbx = await tmx.GetTrackGbxHeaderAsync(track);
        var replayHeader = await tmx.GetReplayGbxHeaderNodeAsync(replay);

        await Assert.That(map.MapUid).IsEqualTo(track.UId);
        await Assert.That(mapGbx.Node.MapUid).IsEqualTo(track.UId);
        await Assert.That(replayHeader.MapInfo?.Id).IsEqualTo(track.UId);
        await Assert.That(replayHeader.Time?.TotalMilliseconds).IsEqualTo(replay.ReplayTime.TotalMilliseconds);
    }

    [Test]
    public async Task MX_MapAndReplayHeaders_MatchTheMap()
    {
        using var mx = new MX(MxSite.Trackmania);

        var map = (await mx.SearchMapsAsync(new MX.SearchMapsParameters { Id = [TrackmaniaMapId] })).Results.Single();
        var replay = (await mx.SearchReplaysAsync(new MX.SearchReplaysParameters { MapId = TrackmaniaMapId })).Results[0];

        var mapHeader = await mx.GetMapGbxHeaderNodeAsync(TrackmaniaMapId);
        var mapGbx = await mx.GetMapGbxHeaderAsync(map);
        var replayHeader = await mx.GetReplayGbxHeaderNodeAsync(replay);

        await Assert.That(mapHeader.MapUid).IsEqualTo(map.MapUid);
        await Assert.That(mapGbx.Node.MapUid).IsEqualTo(map.MapUid);
        await Assert.That(replayHeader.MapInfo?.Id).IsEqualTo(map.MapUid);
    }
}

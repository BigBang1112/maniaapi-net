using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests;

public class MasterServerTMUFTests
{
    private const int LeagueId = 5;

    [Test]
    public async Task GetScoresUrl_CombinesNumberNameAndLeague()
    {
        var url = MasterServerTMUF.GetScoresUrl(ScoresNumber.Scores3, "UnitedRace", LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/UnitedRace/UnitedRace{LeagueId}.gz");
    }

    [Test]
    public async Task GetGeneralScoresUrl_UsesGeneralScores()
    {
        var url = MasterServerTMUF.GetGeneralScoresUrl(ScoresNumber.Scores3, LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/General/General{LeagueId}.gz");
    }

    [Test]
    public async Task GetLadderScoresUrl_UsesMultiScores()
    {
        var url = MasterServerTMUF.GetLadderScoresUrl(ScoresNumber.Scores3, LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/Multi/Multi{LeagueId}.gz");
    }

    [Test]
    public async Task Constructor_Default_UsesUnitedMasterServer()
    {
        using var server = new MasterServerTMUF();

        var baseAddress = server.Client.BaseAddress?.ToString();

        await Assert.That(baseAddress).IsEqualTo(MasterServerTMUF.DefaultUnitedUrl);
    }
    [Test]
    public async Task FetchLatestGeneralScoresInfoAsync_PicksLatestTimestampAndDisposesResponses()
    {
        var contents = new List<TrackedContent>();
        var fake = new FakeHttpMessageHandler(request =>
        {
            // Scores3 has the newest GZip timestamp
            var mtime = request.RequestUri!.AbsolutePath.StartsWith("/scores3/") ? 2_000_000_000u : 1_000_000_000u;
            var content = new TrackedContent(GzipHeader(mtime));
            lock (contents) contents.Add(content);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
        using var server = new MasterServerTMUF(fake.CreateClient(MasterServerTMUF.DefaultUnitedUrl));

        var info = await server.FetchLatestGeneralScoresInfoAsync(LeagueId);

        await Assert.That(info.Number).IsEqualTo(ScoresNumber.Scores3);
        await Assert.That(info.LastModified).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(2_000_000_000));
        await Assert.That(contents.Count).IsEqualTo(6);
        await Assert.That(contents.All(x => x.Disposed)).IsTrue();
    }

    [Test]
    public async Task FetchLatestGeneralScoresInfoAsync_OneRequestFails_ThrowsAndDisposesOtherResponses()
    {
        var contents = new List<TrackedContent>();
        var fake = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.StartsWith("/scores2/"))
            {
                throw new HttpRequestException("Connection reset");
            }

            var content = new TrackedContent(GzipHeader(1_000_000_000u));
            lock (contents) contents.Add(content);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
        using var server = new MasterServerTMUF(fake.CreateClient(MasterServerTMUF.DefaultUnitedUrl));

        await Assert.ThrowsAsync<HttpRequestException>(() => server.FetchLatestGeneralScoresInfoAsync(LeagueId));

        await Assert.That(contents.Count).IsEqualTo(5);
        await Assert.That(contents.All(x => x.Disposed)).IsTrue();
    }

    /// <summary>
    /// The first 10 bytes of a GZip file, with the modification time at offset 4.
    /// </summary>
    private static byte[] GzipHeader(uint mtime)
    {
        var header = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0, 0, 0, 0, 0x00, 0xFF };
        BitConverter.TryWriteBytes(header.AsSpan(4), mtime);
        return header;
    }

    private sealed class TrackedContent(byte[] content) : ByteArrayContent(content)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}

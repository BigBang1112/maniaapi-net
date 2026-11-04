using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests;

public class MasterServerTests
{
    private const string Url = "https://master.example.com/game/request.php";

    [Test]
    public async Task Request_PostsXmlEnvelopeToBaseAddress()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetPlayerInfos", "<a>bigbang1112</a>"));
        using var server = new MasterServerTMUF(fake.CreateClient(Url));

        await server.GetPlayerInfosAsync("bigbang1112");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo(Url);
        await Assert.That(fake.Request.Headers["Content-Type"]).IsEqualTo("text/xml; charset=utf-8");
        await Assert.That(fake.Request.Body).IsEqualTo(
            "<root><game><name>TmForever</name><version>2.11.25</version></game>" +
            "<request><name>GetPlayerInfos</name><params><login>bigbang1112</login></params></request></root>");
    }

    [Test]
    public async Task GetPlayerInfosAsync_ParsesPlayer()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetPlayerInfos",
            "<a>bigbang1112</a><b>$f00Big</b><c>World|Europe|Czech republic</c><d>5</d><e>1199145600</e><j>1609459200</j><k>3</k><x>ignored</x>"));
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        var player = await server.GetPlayerInfosAsync("whatever");

        await Assert.That(player).IsEqualTo(new PlayerInfos(
            Login: "bigbang1112",
            Nickname: "$f00Big",
            Zone: "World|Europe|Czech republic",
            CreatedAt: DateTimeOffset.FromUnixTimeSeconds(1199145600),
            D: 5,
            LastZoneUpdate: DateTimeOffset.FromUnixTimeSeconds(1609459200),
            K: 3));
    }

    [Test]
    public async Task GetPlayerInfosAsync_ZeroCreationTime_IsNull()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetPlayerInfos", "<a>bigbang1112</a><e>0</e>"));
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        var player = await server.GetPlayerInfosAsync("bigbang1112");

        await Assert.That(player.CreatedAt).IsNull();
    }

    [Test]
    public async Task GetPlayerInfosResponseAsync_ParsesExecutionTime()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetPlayerInfos", "<a>bigbang1112</a>", "execution time : 0.5 s"));
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        var response = await server.GetPlayerInfosResponseAsync("bigbang1112");

        await Assert.That(response.ExecutionTime).IsEqualTo(TimeSpan.FromSeconds(0.5));
        await Assert.That(response.Result.Login).IsEqualTo("bigbang1112");
    }

    [Test]
    [Arguments("<e>1</e><p>1</p><m>1</m>", true, true, true)]
    [Arguments("<e>1</e><m>0</m>", true, null, false)]
    [Arguments("<e>0</e><m>0</m>", false, null, false)]
    public async Task CheckLoginAsync_ParsesResult(string content, bool exists, bool? paid, bool migrated)
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("CheckLogin", content));
        using var server = new MasterServerTMUF(fake.CreateClient(Url));

        var result = await server.CheckLoginAsync("bigbang1112");

        await Assert.That(fake.Request.Body).Contains("<request><name>CheckLogin</name><params><l>bigbang1112</l></params></request>");
        await Assert.That(result).IsEqualTo(new CheckLoginResult(exists, paid, migrated));
    }

    [Test]
    public async Task GetLeaguesAsync_ParsesLeagues()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetLeagues",
            "<l><a>Europe</a><b>World</b><i>https://example.com/europe.dds</i></l>" +
            "<l><a>Czech republic</a><b>World|Europe</b><i></i></l>"));
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        var leagues = await server.GetLeaguesAsync();

        await Assert.That(leagues).IsEquivalentTo(
        [
            new League("Europe", "World", "https://example.com/europe.dds"),
            new League("Czech republic", "World|Europe", "")
        ]);
        await Assert.That(leagues[1].GetFullPath()).IsEqualTo("World|Europe|Czech republic");
    }

    [Test]
    public async Task ErrorResponse_ThrowsXmlRequestException()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Error("GetPlayerInfos", 12, "Unknown player"));
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        var ex = await Assert.ThrowsAsync<XmlRequestException>(() => server.GetPlayerInfosAsync("nobody"));

        await Assert.That(ex!.Value).IsEqualTo(12);
        await Assert.That(ex.ErrorMessage).IsEqualTo("Unknown player");
        await Assert.That(ex.Message).IsEqualTo("Error 12: Unknown player");
    }

    [Test]
    public async Task HttpError_Throws()
    {
        var fake = FakeHttpMessageHandler.Text("", "text/xml", HttpStatusCode.ServiceUnavailable);
        using var server = new MasterServerMP4(fake.CreateClient(Url));

        await Assert.ThrowsAsync<HttpRequestException>(() => server.GetLeaguesAsync());
    }

    [Test]
    public async Task TestAsync_ReadsExecutionTimeWithoutContent()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Empty("execution time : 0.002 s"));
        using var server = new MasterServerTMUF(fake.CreateClient(Url));

        var response = await server.TestAsync();

        await Assert.That(fake.Request.Body).Contains("<request><name>Test</name><params></params></request>");
        await Assert.That(response.ExecutionTime).IsEqualTo(TimeSpan.FromSeconds(0.002));
    }

    [Test]
    public async Task UnexpectedRootElement_Throws()
    {
        var fake = MasterServerXml.Handler("<html><body>Maintenance</body></html>");
        using var server = new MasterServerTMUF(fake.CreateClient(Url));

        await Assert.ThrowsAsync<Exception>(() => server.TestAsync());
    }

    [Test]
    public async Task Constructor_ClientWithoutBaseAddress_Throws()
    {
        await Assert.That(() => new MasterServerTMUF(new HttpClient())).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_AddsLibraryUserAgentOnce()
    {
        using var http = new HttpClient { BaseAddress = new Uri(Url) };

        _ = new MasterServerTMUF(http);
        _ = new MasterServerMP4(http);

        var products = http.DefaultRequestHeaders.UserAgent.Where(x => x.Product?.Name == "ManiaAPI.NET");
        await Assert.That(products).Count().IsEqualTo(1);
    }
}

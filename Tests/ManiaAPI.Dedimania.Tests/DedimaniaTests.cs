using TmEssentials;

namespace ManiaAPI.Dedimania.Tests;

public class DedimaniaTests
{
    // A01-Race, the first official TMNF map.
    private const string Uid = "BeySZdnfuSh4nHY5xztiXLmlrXe";

    // Copied from real replies, which separate lines with CRLF. The second record has no date and server login, as
    // without "more". Its server name had a comma, which Dedimania replaced with a dot.
    private const string MapResponse =
        "BeySZdnfuSh4nHY5xztiXLmlrXe,Stadium,Nadeo,1162,A01-Race\r\n" +
        "23966,2,1614365506,sawken7,droppie_lolmaps,$000*$000$f22aAa$000*$i$f22 »$fffмα$f22ॅ$fffх,$00F$W$OLOLMAPS$FFF\r\n" +
        "24004,1,,trabadia,,$ffftrabadia,$F00爪$E00尺.$D00乃\r\n";

    [Test]
    public async Task GetMapAsync_RequestsMapWithMoreDetails()
    {
        var fake = FakeHttpMessageHandler.Text(MapResponse);
        using var dedimania = new Dedimania(fake.CreateClient());

        await dedimania.GetMapAsync(Uid);

        await Assert.That(fake.Request.Url).IsEqualTo($"http://dedimania.net:8080/MAP?uid={Uid}&more");
    }

    [Test]
    [Arguments(GameMode.Rounds, 1)]
    [Arguments(GameMode.TimeAttack, 2)]
    public async Task GetMapAsync_Mode_RequestsMode(GameMode mode, int expected)
    {
        var fake = FakeHttpMessageHandler.Text(MapResponse);
        using var dedimania = new Dedimania(fake.CreateClient());

        await dedimania.GetMapAsync(Uid, mode);

        await Assert.That(fake.Request.Url).IsEqualTo($"http://dedimania.net:8080/MAP?uid={Uid}&mode={expected}&more");
    }

    [Test]
    public async Task GetMapAsync_EscapesUid()
    {
        var fake = FakeHttpMessageHandler.Text("-1");
        using var dedimania = new Dedimania(fake.CreateClient());

        await dedimania.GetMapAsync("a&b c");

        // Uri.ToString() unescapes %20 for display, AbsoluteUri is what is sent.
        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo("http://dedimania.net:8080/MAP?uid=a%26b%20c&more");
    }

    [Test]
    public async Task GetMapAsync_ReturnsParsedMap()
    {
        var fake = FakeHttpMessageHandler.Text(MapResponse);
        using var dedimania = new Dedimania(fake.CreateClient());

        var map = await dedimania.GetMapAsync(Uid);

        await Assert.That(map).IsNotNull();
        await Assert.That(map!.Uid).IsEqualTo(Uid);
        await Assert.That(map.Environment).IsEqualTo("Stadium");
        await Assert.That(map.AuthorLogin).IsEqualTo("Nadeo");
        await Assert.That(map.TimesPlayed).IsEqualTo(1162);
        await Assert.That(map.Name).IsEqualTo("A01-Race");
        await Assert.That(map.Records).IsEquivalentTo([
            new MapRecord(new TimeInt32(23966), GameMode.TimeAttack, DateTimeOffset.FromUnixTimeSeconds(1614365506), "sawken7", "droppie_lolmaps", "$000*$000$f22aAa$000*$i$f22 »$fffмα$f22ॅ$fffх", "$00F$W$OLOLMAPS$FFF"),
            new MapRecord(new TimeInt32(24004), GameMode.Rounds, null, "trabadia", null, "$ffftrabadia", "$F00爪$E00尺.$D00乃")
        ]);
    }

    [Test]
    public async Task GetMapAsync_NoRecords_ReturnsEmptyRecords()
    {
        var fake = FakeHttpMessageHandler.Text("QfWgfkZpeKY9h5WrtrnnOsRdQOb,Canyon,old_bug,14,2 Crazy Mini\r\n");
        using var dedimania = new Dedimania(fake.CreateClient());

        var map = await dedimania.GetMapAsync("QfWgfkZpeKY9h5WrtrnnOsRdQOb", GameMode.TimeAttack);

        await Assert.That(map).IsNotNull();
        await Assert.That(map!.Records).IsEmpty();
    }

    [Test]
    public async Task GetMapAsync_UnknownMap_ReturnsNull()
    {
        var fake = FakeHttpMessageHandler.Text("-1\r\n");
        using var dedimania = new Dedimania(fake.CreateClient());

        var map = await dedimania.GetMapAsync("nothing");

        await Assert.That(map).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task GetMapAsync_EmptyUid_Throws(string uid)
    {
        var fake = FakeHttpMessageHandler.Text(MapResponse);
        using var dedimania = new Dedimania(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => dedimania.GetMapAsync(uid));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetMapAsync_UnexpectedResponse_Throws()
    {
        var fake = FakeHttpMessageHandler.Text("usage: http://dedimania.net:8080/MAP?uid=...\r\n");
        using var dedimania = new Dedimania(fake.CreateClient());

        await Assert.ThrowsAsync<FormatException>(() => dedimania.GetMapAsync(Uid));
    }

    [Test]
    public async Task GetMapAsync_ServerError_Throws()
    {
        var fake = FakeHttpMessageHandler.Text("", statusCode: HttpStatusCode.InternalServerError);
        using var dedimania = new Dedimania(fake.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(() => dedimania.GetMapAsync(Uid));
    }
}

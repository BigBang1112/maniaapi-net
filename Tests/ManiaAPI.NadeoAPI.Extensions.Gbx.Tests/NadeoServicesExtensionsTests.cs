using GBX.NET;
using GBX.NET.Engines.Game;
using System.Text.RegularExpressions;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx.Tests;

public partial class NadeoServicesExtensionsTests
{
    private const string MapFileName = "ManiaAPI-NET Test Map.Map.Gbx";
    private const string BaseUrl = "https://prod.trackmania.core.nadeo.online";
    private const string OtherMapUid = "uid";

    private static readonly Guid MapId = Guid.Parse("ed53486f-a80d-419b-b073-54bb8944d502");

    private static string MapPath => Path.Combine(AppContext.BaseDirectory, MapFileName);

    private static CGameCtnChallenge ReadMapHeader() => GBX.NET.Gbx.ParseHeaderNode<CGameCtnChallenge>(MapPath);

    private static string MapInfoJson(string mapUid) => $$"""
        {
          "author": "{{Guid.NewGuid()}}",
          "authorScore": 3572,
          "bronzeScore": 6000,
          "collectionName": "Stadium",
          "filename": "{{MapFileName}}",
          "goldScore": 4000,
          "isPlayable": true,
          "mapId": "{{MapId}}",
          "mapStyle": "",
          "mapType": "TrackMania\\TM_Race",
          "mapUid": "{{mapUid}}",
          "name": "Test map",
          "silverScore": 5000,
          "submitter": "{{Guid.NewGuid()}}",
          "timestamp": "2024-05-01T12:00:00+00:00",
          "fileUrl": "https://example.com/map.Map.Gbx",
          "thumbnailUrl": "https://example.com/map.jpg"
        }
        """;

    [GeneratedRegex("""Content-Disposition: form-data; name="?(?<name>[^";\r\n]+)"?(?<rest>[^\r\n]*)\r\n(?:[^\r\n]+\r\n)*\r\n(?<value>.*?)\r\n--""", RegexOptions.Singleline)]
    private static partial Regex FormPartRegex();

    /// <summary>
    /// Reads the text fields of a multipart/form-data body, and the rest of the Content-Disposition header of each part.
    /// </summary>
    private static Dictionary<string, (string Value, string Disposition)> ReadForm(string body)
    {
        return FormPartRegex().Matches(body).ToDictionary(
            x => x.Groups["name"].Value,
            x => (x.Groups["value"].Value, x.Groups["rest"].Value));
    }

    [Test]
    public async Task UploadMapAsync_PostsMultipartFormToMaps()
    {
        var map = ReadMapHeader();
        var fake = FakeHttpMessageHandler.Json(MapInfoJson(map.MapUid));
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await ns.UploadMapAsync(stream, MapFileName);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/maps/");
        await Assert.That(fake.Request.Headers["Content-Type"]).StartsWith("multipart/form-data");
    }

    [Test]
    public async Task UploadMapAsync_SendsMapFieldsFromHeader()
    {
        var map = ReadMapHeader();
        var fake = FakeHttpMessageHandler.Json(MapInfoJson(map.MapUid));
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await ns.UploadMapAsync(stream, MapFileName);

        var form = ReadForm(fake.Request.Body!);
        await Assert.That(form["mapUid"].Value).IsEqualTo(map.MapUid);
        await Assert.That(form["name"].Value).IsEqualTo(map.MapName);
        await Assert.That(form["authorScore"].Value).IsEqualTo(map.AuthorTime!.Value.TotalMilliseconds.ToString());
        await Assert.That(form["collectionName"].Value).IsEqualTo("Stadium");
        await Assert.That(form["nadeoservices-core-parameters"].Value).IsEqualTo("""{"isPlayable":true}""");
        await Assert.That(form["data"].Disposition).Contains(MapFileName);
    }

    [Test]
    public async Task UploadMapAsync_ReturnsUploadedMapInfo()
    {
        var map = ReadMapHeader();
        var fake = FakeHttpMessageHandler.Json(MapInfoJson(map.MapUid));
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        var result = await ns.UploadMapAsync(stream, MapFileName);

        await Assert.That(result.MapId).IsEqualTo(MapId);
    }

    [Test]
    public async Task UploadMapAsync_FromFilePath_UsesFileName()
    {
        var fake = FakeHttpMessageHandler.Json(MapInfoJson(OtherMapUid));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.UploadMapAsync(MapPath);

        await Assert.That(ReadForm(fake.Request.Body!)["data"].Disposition).Contains(MapFileName);
    }

    [Test]
    public async Task UpdateMapAsync_WithMapId_PostsToMap()
    {
        var fake = FakeHttpMessageHandler.Json(MapInfoJson(OtherMapUid));
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await ns.UpdateMapAsync(MapId, stream, MapFileName);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/maps/{MapId}");
    }

    [Test]
    public async Task UpdateMapAsync_WithoutMapId_LooksUpMapIdByUid()
    {
        var map = ReadMapHeader();
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json($"[{MapInfoJson(map.MapUid)}]"),
            () => FakeResponse.Json(MapInfoJson(map.MapUid)));
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await ns.UpdateMapAsync(stream, MapFileName);

        await Assert.That(fake.Requests[0].Url).IsEqualTo($"{BaseUrl}/maps/by-uid/?mapUidList={map.MapUid}");
        await Assert.That(fake.Requests[1].Url).IsEqualTo($"{BaseUrl}/maps/{MapId}");
    }

    [Test]
    public async Task UpdateMapAsync_UnknownMap_ThrowsMapNotFoundWithoutUploading()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await Assert.ThrowsAsync<MapNotFoundException>(() => ns.UpdateMapAsync(stream, MapFileName));

        await Assert.That(fake.Requests).Count().IsEqualTo(1);
    }

    [Test]
    public async Task UploadMapAsync_NullStream_ThrowsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentNullException>(() => ns.UploadMapAsync((Stream)null!, MapFileName));

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task UploadMapAsync_ApiError_ThrowsResponseException()
    {
        var fake = FakeHttpMessageHandler.Json("""{"code":"","correlation_id":"","message":"Forbidden","info":{}}""", HttpStatusCode.Forbidden);
        using var ns = new NadeoServices(fake.CreateClient());
        await using var stream = File.OpenRead(MapPath);

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.UploadMapAsync(stream, MapFileName));
    }
}

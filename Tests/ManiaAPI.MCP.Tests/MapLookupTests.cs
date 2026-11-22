using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Nodes;

namespace ManiaAPI.MCP.Tests;

public class MapLookupTests
{
    private const string Uid = "BeySZdnfuSh4nHY5xztiXLmlrXe";

    private const string Tracks = $$"""
        {"More":true,"Results":[{"TrackId":2233,"TrackName":"$fffA01-Race","UId":"{{Uid}}","Uploader":{"UserId":1,"Name":"Nadeo"},
        "UploadedAt":"2008-04-16T00:00:00+00:00","UpdatedAt":"2008-04-16T00:00:00+00:00","Environment":1}]}
        """;

    private const string Maps = """
        {"More":false,"Results":[{"MapId":5,"MapUid":"mxuid","Name":"A01 Remake","Uploader":{"UserId":2,"Name":"mapper"},
        "UploadedAt":"2020-07-01T00:00:00+00:00","UpdatedAt":"2020-07-01T00:00:00+00:00","MapType":"TM_Race"}]}
        """;

    [Test]
    public async Task UidLookupSearchesOnlyTheSitesOfTheGame()
    {
        var fake = Exchanges();
        using var services = CreateServices(fake);

        var result = await LookupAsync(services, $$"""{"uid":"{{Uid}}","game":"tmnf"}""");

        await Assert.That(fake.Urls.Count).IsEqualTo(1);
        await Assert.That(fake.Urls.Single()).StartsWith("https://tmnf.exchange/api/tracks?");
        await Assert.That(fake.Urls.Single()).Contains($"uid={Uid}");

        var match = result["data"]!["matches"]![0]!;
        await Assert.That(match["site"]!.GetValue<string>()).IsEqualTo("TMNF");
        await Assert.That(match["id"]!.GetValue<long>()).IsEqualTo(2233);
        await Assert.That(match["uid"]!.GetValue<string>()).IsEqualTo(Uid);
        await Assert.That(match["name"]!.GetValue<string>()).IsEqualTo("$fffA01-Race");
        await Assert.That(match["namePlain"]!.GetValue<string>()).IsEqualTo("A01-Race");
        await Assert.That(match["author"]!.GetValue<string>()).IsEqualTo("Nadeo");
        await Assert.That(Notes(result)).Contains("TMNF has more matches; raise count or narrow name and author.");
    }

    [Test]
    public async Task NameLookupSearchesEverySiteInScopeInOneShape()
    {
        var fake = Exchanges();
        using var services = CreateServices(fake);
        var options = services.GetRequiredService<McpOptions>();

        var result = await LookupAsync(services, """{"name":"A01","count":3}""");
        var matches = result["data"]!["matches"]!.AsArray();

        await Assert.That(fake.Urls.Count).IsEqualTo(options.Exchange.Sites.Length + options.Exchange.MxSites.Length);
        await Assert.That(fake.Urls.All(url => url.Contains("name=A01", StringComparison.Ordinal) && url.Contains("count=3", StringComparison.Ordinal))).IsTrue();
        await Assert.That(matches.Count).IsEqualTo(fake.Urls.Count);

        var mx = matches.Single(match => match!["site"]!.GetValue<string>() == "Trackmania")!;
        await Assert.That(mx["id"]!.GetValue<long>()).IsEqualTo(5);
        await Assert.That(mx["uid"]!.GetValue<string>()).IsEqualTo("mxuid");
        await Assert.That(mx["name"]!.GetValue<string>()).IsEqualTo("A01 Remake");
        await Assert.That(mx["author"]!.GetValue<string>()).IsEqualTo("mapper");
    }

    [Test]
    public async Task FailingSiteLeavesANoteAndKeepsTheOthers()
    {
        var fake = Exchanges(failHost: "trackmania.exchange");
        using var services = CreateServices(fake);

        var result = await LookupAsync(services, """{"author":"mapper","game":"maniaplanet"}""");
        var matches = result["data"]!["matches"]!.AsArray();

        await Assert.That(matches.Select(match => match!["site"]!.GetValue<string>())).IsEquivalentTo(["Maniaplanet", "Shootmania"]);

        var failing = await LookupAsync(services, """{"author":"mapper","site":"Trackmania"}""");
        await Assert.That(failing["data"]!["matches"]!.AsArray()).IsEmpty();
        await Assert.That(Notes(failing).Any(note => note.StartsWith("Trackmania unavailable:", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task UnknownUidSaysTheMapMayNotBeOnAnExchange()
    {
        var fake = new ConcurrentFakeHttpMessageHandler(_ => Task.FromResult(FakeResponse.Json("""{"More":false,"Results":[]}""")));
        using var services = CreateServices(fake);

        var result = await LookupAsync(services, """{"uid":"nothing","game":"tm2"}""");

        await Assert.That(result["data"]!["matches"]!.AsArray()).IsEmpty();
        await Assert.That(Notes(result)).Contains("No exchange site in scope has this UID; the map may not be uploaded to an exchange.");
    }

    [Test]
    [Arguments("""{}""")]
    [Arguments("""{"game":"tmnf","name":" "}""")]
    public async Task LookupRequiresUidNameOrAuthor(string arguments)
    {
        var fake = Exchanges();
        using var services = CreateServices(fake);

        await Assert.That(async () => await LookupAsync(services, arguments))
            .Throws<ArgumentException>().WithMessageContaining("Supply uid");
        await Assert.That(fake.Urls).IsEmpty();
    }

    [Test]
    public async Task GameWithoutExchangeSitesIsRefused()
    {
        var fake = Exchanges();
        using var services = CreateServices(fake);

        await Assert.That(async () => await LookupAsync(services, """{"name":"A01","game":"trackmaniaturbo"}"""))
            .Throws<ArgumentException>().WithMessageContaining("No exchange site");
        await Assert.That(fake.Urls).IsEmpty();
    }

    [Test]
    [Arguments("TMNF", "https://tmnf.exchange/api/tracks?")]
    [Arguments("Trackmania", "https://trackmania.exchange/api/maps?")]
    public async Task ExchangeSearchFiltersByUid(string site, string url)
    {
        var fake = Exchanges();
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        await services.GetRequiredService<FeaturedTools>().InvokeAsync("exchange_search",
            EngineTests.Json($$$"""{"site":"{{{site}}}","parameters":{"uid":["{{{Uid}}}"]}}"""),
            GameScope.Create(services.GetRequiredService<McpOptions>()), request.ServiceProvider, CancellationToken.None);

        await Assert.That(fake.Urls.Single()).StartsWith(url);
        await Assert.That(fake.Urls.Single()).Contains($"uid={Uid}");
    }

    [Test]
    public async Task LookupIsListedWithTheExchangeTools()
    {
        using var services = CreateServices(Exchanges());

        var tool = services.GetRequiredService<FeaturedTools>()
            .List(GameScope.Create(services.GetRequiredService<McpOptions>(), "tmnf"))
            .Single(tool => tool.Name == "map_lookup");

        await Assert.That(tool.InputSchema.GetProperty("properties").GetProperty("site").GetProperty("enum").GetRawText())
            .IsEqualTo("""["TMNF"]""");
    }

    /// <summary>
    /// Answers TMX track searches and MX map searches with one result each, or a server error on one host.
    /// </summary>
    private static ConcurrentFakeHttpMessageHandler Exchanges(string? failHost = null)
    {
        return new ConcurrentFakeHttpMessageHandler(request => Task.FromResult(
            request.RequestUri!.Host == failHost ? FakeResponse.Status(HttpStatusCode.InternalServerError) :
            request.RequestUri.AbsolutePath.EndsWith("/api/tracks", StringComparison.Ordinal) ? FakeResponse.Json(Tracks) :
            FakeResponse.Json(Maps)));
    }

    private static async Task<JsonObject> LookupAsync(ServiceProvider services, string arguments)
    {
        using var request = services.CreateScope();

        return await services.GetRequiredService<FeaturedTools>().InvokeAsync("map_lookup", EngineTests.Json(arguments),
            GameScope.Create(services.GetRequiredService<McpOptions>()), request.ServiceProvider, CancellationToken.None);
    }

    private static string[] Notes(JsonObject result)
    {
        return result["truncation"]!.AsArray().Select(note => note!.GetValue<string>()).ToArray();
    }

    private static ServiceProvider CreateServices(ConcurrentFakeHttpMessageHandler fake)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create());
        collection.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => fake));

        return collection.BuildServiceProvider();
    }
}

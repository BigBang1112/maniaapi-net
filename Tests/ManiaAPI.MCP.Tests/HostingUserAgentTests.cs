using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;
using ManiaAPI.MasterServer;
using ManiaAPI.MasterServer.Extensions.Hosting;
using ManiaAPI.MasterServer.MP3;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;
using ManiaAPI.MasterServer.TMUF;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Hosting;
using ManiaAPI.TMX;
using ManiaAPI.TMX.Extensions.Hosting;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;
using ManiaAPI.TrackmaniaWS;
using ManiaAPI.TrackmaniaWS.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class HostingUserAgentTests
{
    [Test]
    public async Task EveryHostingRegistration_SendsApplicationIdentityFirst()
    {
        const string xml = "<r><r><n>GetWaitingParams</n><c><ms><b>Relay</b><c>relay.example.test</c><d>game</d></ms></c></r><e>execution time : 0.0123 s</e></r>";
        var fake = FakeHttpMessageHandler.Text(xml, "text/xml");
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureHttpClientDefaults(builder => builder
            .ConfigureHttpClient(client => client.DefaultRequestHeaders.UserAgent.ParseAdd("ExistingApp/2.0"))
            .ConfigurePrimaryHttpMessageHandler(() => fake));
        services.AddNadeoAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddTrackmaniaAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddManiaPlanetAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddManiaPlanetIngameAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddManiaPlanetChannelRecordAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddManiaPlanetChannelFeedbackAPI(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddTrackmaniaWS(new TrackmaniaWSOptions
        {
            Credentials = new("login", "password"),
            UserAgent = UserAgentAssertions.Value
        });
        services.AddTMX(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddMasterServerTMUF(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddMasterServerMP3(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddMasterServerMP4(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddMasterServerTMT(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddHttpClient(nameof(MasterServerMP3)).ConfigurePrimaryHttpMessageHandler(() => fake);
        services.AddHttpClient(nameof(MasterServerMP4)).ConfigurePrimaryHttpMessageHandler(() => fake);
        foreach (var platform in Enum.GetValues<Platform>())
        {
            services.AddHttpClient($"{nameof(MasterServerTMT)}_{platform}").ConfigurePrimaryHttpMessageHandler(() => fake);
        }

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;
        List<HttpClient> clients =
        [
            scoped.GetRequiredService<NadeoServices>().Client,
            scoped.GetRequiredService<NadeoLiveServices>().Client,
            scoped.GetRequiredService<NadeoMeetServices>().Client,
            scoped.GetRequiredService<TrackmaniaAPI.TrackmaniaAPI>().Client,
            scoped.GetRequiredService<ManiaPlanetAPI.ManiaPlanetAPI>().Client,
            scoped.GetRequiredService<ManiaPlanetAPI.ManiaPlanetIngameAPI>().Client,
            scoped.GetRequiredService<ManiaPlanetAPI.ManiaPlanetChannelRecordAPI>().Client,
            scoped.GetRequiredService<ManiaPlanetAPI.ManiaPlanetChannelFeedbackAPI>().Client,
            scoped.GetRequiredService<TrackmaniaWS.TrackmaniaWS>().Client,
            scoped.GetRequiredService<IX>().Client,
            scoped.GetRequiredService<MasterServerTMUF>().Client,
            scoped.GetRequiredService<InitServerMP3>().Client,
            scoped.GetRequiredService<InitServerMP4>().Client,
            scoped.GetRequiredService<MasterServerMP3>().Client,
            scoped.GetRequiredService<MasterServerMP4>().Client
        ];
        clients.AddRange(Enum.GetValues<TmxSite>().Select(site => scoped.GetRequiredKeyedService<TMX.TMX>(site).Client));
        clients.AddRange(Enum.GetValues<MxSite>().Select(site => scoped.GetRequiredKeyedService<MX>(site).Client));
        clients.AddRange(Enum.GetValues<Platform>().Select(platform => scoped.GetRequiredKeyedService<InitServerTMT>(platform).Client));

        var info = new MasterServerInfo("Relay", "relay.example.test", "game", 443, 80);
        using var mp3 = scoped.GetRequiredService<IMasterServerMP3Factory>().CreateClient(info);
        using var mp4 = scoped.GetRequiredService<IMasterServerMP4Factory>().CreateClient(info);
        clients.Add(mp3.Client);
        clients.Add(mp4.Client);

        var turboFactory = scoped.GetRequiredService<IMasterServerTMTFactory>();
        await turboFactory.RequestWaitingParamsAsync();
        var turboClients = turboFactory.CreateClients();
        clients.AddRange(turboClients.Values.Select(server => server.Client));

        foreach (var client in clients)
        {
            await UserAgentAssertions.VerifyAsync(client, replacesExisting: true);
            using var response = await client.GetAsync("https://example.test/");
            await Assert.That(fake.Requests[^1].Headers["User-Agent"]).StartsWith("MyApp/1.0");
        }
        foreach (var server in turboClients.Values)
        {
            server.Dispose();
        }
    }

    [Test]
    public async Task IndividualNadeoRegistrations_ConfigureAllAudiences()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNadeoServices(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddNadeoLiveServices(options => options.UserAgent = UserAgentAssertions.Value);
        services.AddNadeoMeetServices(options => options.UserAgent = UserAgentAssertions.Value);
        using var provider = services.BuildServiceProvider();

        await UserAgentAssertions.VerifyAsync(provider.GetRequiredService<NadeoServices>().Client);
        await UserAgentAssertions.VerifyAsync(provider.GetRequiredService<NadeoLiveServices>().Client);
        await UserAgentAssertions.VerifyAsync(provider.GetRequiredService<NadeoMeetServices>().Client);
    }
}

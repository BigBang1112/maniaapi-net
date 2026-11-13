using ManiaAPI.MCP.Configuration;
using ManiaAPI.NadeoAPI;
using ManiaAPI.TMX;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MasterServer.MP3;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMUF;
using ManiaAPI.MCP.Resources;
using System.Text.Json.Nodes;
using System.Text.Json;
using ManiaAPI.MCP.Accounts;

namespace ManiaAPI.MCP.Catalog;

public sealed class InvocationContext(
    IServiceProvider services,
    INadeoAccount nadeo,
    string? site,
    string? platform = null,
    string? resourceUri = null,
    BinaryCapture? capture = null,
    Operation? operation = null)
{
    public IServiceProvider Services => services;
    public string? Site => site;

    public async Task<JsonObject> ScoresAsync(string prefix, string method, JsonElement arguments, CancellationToken cancellationToken)
    {
        var client = Client<MasterServerTMUF>(prefix);
        var scoresName = method.Contains("Campaign", StringComparison.Ordinal)
            ? arguments.GetProperty("campaignName").GetString()!
            : method.Contains("General", StringComparison.Ordinal) ? "General" : "Multi";

        var leagueId = arguments.TryGetProperty("leagueId", out var id)
            ? id.GetInt32()
            : MasterServer.League.IdsWithDataInTMUF.TryGetValue(arguments.GetProperty("league").GetString()!, out var found)
                ? found
                : throw new ArgumentException("The selected league has no scores file.");

        var number = method.Contains("Latest", StringComparison.Ordinal)
            ? (scoresName == "General"
                ? await client.FetchLatestGeneralScoresInfoAsync(leagueId, cancellationToken)
                : scoresName == "Multi"
                    ? await client.FetchLatestLadderScoresInfoAsync(leagueId, cancellationToken)
                    : await client.FetchLatestCampaignScoresInfoAsync(scoresName, leagueId, cancellationToken)).Number
            : CatalogMetadata.Argument<MasterServer.TMUF.ScoresNumber>(arguments, "num");

        if (capture is null)
        {
            var metadata = BinarySummary();
            metadata["url"] = MasterServerTMUF.GetScoresUrl(number, scoresName, leagueId);

            return metadata;
        }

        using var stream = await client.DownloadScoresAsync(number, scoresName, leagueId, cancellationToken);

        return await BinaryAsync(stream, cancellationToken);
    }

    public JsonObject BinarySummary()
    {
        return new JsonObject
        {
            ["resourceUri"] = resourceUri,
            ["contentType"] = "application/octet-stream",
            ["summary"] = "Binary scores data; read the resource to download the file."
        };
    }

    public async Task<JsonObject> BinaryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();

        if (capture is not null)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await CaptureAsync(stream, cancellationToken);
            capture.ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        }

        return new JsonObject
        {
            ["status"] = (int)response.StatusCode,
            ["contentType"] = response.Content.Headers.ContentType?.MediaType,
            ["length"] = response.Content.Headers.ContentLength,
            ["url"] = response.RequestMessage?.RequestUri?.ToString(),
            ["resourceUri"] = resourceUri
        };
    }

    public async Task<JsonObject> BinaryAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (capture is not null)
        {
            await CaptureAsync(stream, cancellationToken);
            capture.ContentType = "application/octet-stream";
        }

        return BinarySummary();
    }

    private async Task CaptureAsync(Stream stream, CancellationToken cancellationToken)
    {
        var limit = services.GetRequiredService<McpOptions>().Shaping.MaxResourceBytes;
        using var buffer = new MemoryStream();
        var block = new byte[81920];

        while (true)
        {
            var count = await stream.ReadAsync(block, cancellationToken);

            if (count == 0)
            {
                break;
            }

            if (buffer.Length + count > limit)
            {
                throw new ArgumentException("The resource exceeds MaxResourceBytes.");
            }

            await buffer.WriteAsync(block.AsMemory(0, count), cancellationToken);
        }

        capture!.Bytes = buffer.ToArray();
    }

    public async Task<T> ClientAsync<T>(string prefix, CancellationToken cancellationToken) where T : class
    {
        if (operation?.PlayerOnly == true)
        {
            return (T)await services.GetRequiredService<LinkedClients>().GetAsync(operation, cancellationToken);
        }

        if (prefix is "masterserver.tmt" or "masterserver.tmt_aggregated")
        {
            return (T)await services.GetRequiredService<LegacyClients>().TurboAsync(
                prefix == "masterserver.tmt" ? platform : null, cancellationToken);
        }

        return Client<T>(prefix);
    }

    public T Client<T>(string prefix) where T : class
    {
        object client = prefix switch
        {
            "nadeo.core" => nadeo.Services.GetRequiredService<NadeoServices>(),
            "nadeo.live" => nadeo.Services.GetRequiredService<NadeoLiveServices>(),
            "nadeo.meet" => nadeo.Services.GetRequiredService<NadeoMeetServices>(),
            "trackmania.api" => services.GetRequiredService<TrackmaniaAPI.TrackmaniaAPI>(),
            "tmx" => services.GetRequiredKeyedService<TMX.TMX>(Enum.Parse<TmxSite>(site!)),
            "mx" => services.GetRequiredKeyedService<MX>(Enum.Parse<MxSite>(site!)),
            "masterserver.tmuf" => services.GetRequiredService<MasterServerTMUF>(),
            "masterserver.tmnf" => services.GetRequiredKeyedService<MasterServerTMUF>("tmnf"),
            "masterserver.mp3" => services.GetRequiredService<MasterServerMP3>(),
            "masterserver.mp4" => operation?.Id is "masterserver.mp4.player_infos"
                ? services.GetRequiredKeyedService<MasterServerMP4>("player")
                : services.GetRequiredService<MasterServerMP4>(),
            "maniaplanet.web" => services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetAPI>(),
            "maniaplanet.ingame" => services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetIngameAPI>(),
            "maniaplanet.channel_record" => services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetChannelRecordAPI>(),
            "maniaplanet.channel_feedback" => services.GetRequiredService<ManiaPlanetAPI.ManiaPlanetChannelFeedbackAPI>(),
            "trackmania.ws" => services.GetRequiredService<TrackmaniaWS.TrackmaniaWS>(),
            "united.ladder" => services.GetRequiredService<UnitedLadder.UnitedLadder>(),
            _ => throw new InvalidOperationException("Unknown catalog client.")
        };

        return (T)client;
    }
}

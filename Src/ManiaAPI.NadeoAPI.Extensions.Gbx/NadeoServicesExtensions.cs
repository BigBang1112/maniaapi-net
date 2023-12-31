using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI.JsonContexts;
using System.Net.Http.Json;
using TmEssentials;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

public static class NadeoServicesExtensions
{
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, Stream stream, CancellationToken cancellationToken = default)
    {
        using var bufferedStream = new BufferedStream(stream);

        var map = GameBox.ParseNodeHeader<CGameCtnChallenge>(bufferedStream);

        bufferedStream.Position = 0;

        var content = new MultipartFormDataContent
        {
            { new StringContent(map.TMObjective_AuthorTime.GetValueOrDefault().TotalMilliseconds.ToString()), "authorScore" },
            { new StringContent(map.TMObjective_GoldTime.GetValueOrDefault().TotalMilliseconds.ToString()), "goldScore" },
            { new StringContent(map.TMObjective_SilverTime.GetValueOrDefault().TotalMilliseconds.ToString()), "silverScore" },
            { new StringContent(map.TMObjective_BronzeTime.GetValueOrDefault().TotalMilliseconds.ToString()), "bronzeScore" },
            { new StringContent(AccountUtils.ToAccountId(map.AuthorLogin).ToString()), "author" },
            { new StringContent(map.Collection == "Stadium2020" ? "Stadium" : map.Collection), "collectionName" },
            { new StringContent(map.MapStyle ?? string.Empty), "mapStyle" },
            { new StringContent(map.MapType ?? string.Empty), "mapType" },
            { new StringContent(map.MapUid), "mapUid" },
            { new StringContent(map.MapName), "name" },
            { new StringContent("true"), "isPlayable" },
            { new StreamContent(bufferedStream), "data", "path" }
        };

        using var response = await services.SendAsync(HttpMethod.Post, "maps", content, cancellationToken);

        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }
}

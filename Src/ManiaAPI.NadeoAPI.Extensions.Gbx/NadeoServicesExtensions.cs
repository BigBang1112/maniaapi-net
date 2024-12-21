using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI.JsonContexts;
using System.Net.Http.Json;
using TmEssentials;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

/// <summary>
/// Provides Gbx related extensions to <see cref="INadeoServices"/>.
/// </summary>
public static class NadeoServicesExtensions
{
    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        using var response = await services.SendAsync(HttpMethod.Post, "maps/", content, cancellationToken);

        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, FileStream stream, CancellationToken cancellationToken = default)
    {
        return await UploadMapAsync(services, stream, Path.GetFileName(stream.Name), cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        return await UploadMapAsync(services, stream, Path.GetFileName(filePath), cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);

        using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        using var response = await services.SendAsync(HttpMethod.Post, $"maps/{mapId}", content, cancellationToken);

        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, FileStream stream, CancellationToken cancellationToken = default)
    {
        return await UpdateMapAsync(services, mapId, stream, Path.GetFileName(stream.Name), cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        return await UpdateMapAsync(services, mapId, stream, Path.GetFileName(filePath), cancellationToken);
    }

    private static MultipartFormDataContent CreateContent(BufferedStream bufferedStream, string fileName)
    {
        var map = GBX.NET.Gbx.ParseHeaderNode<CGameCtnChallenge>(bufferedStream);

        bufferedStream.Position = 0;

        var mapInfo = new MapInfoSubmit(
            map.AuthorTime.GetValueOrDefault().TotalMilliseconds,
            map.GoldTime.GetValueOrDefault().TotalMilliseconds,
            map.SilverTime.GetValueOrDefault().TotalMilliseconds,
            map.BronzeTime.GetValueOrDefault().TotalMilliseconds,
            AccountUtils.ToAccountId(map.AuthorLogin),
            map.Collection.HasValue && map.Collection.Value.Number != 26 ? map.Collection : "Stadium",
            map.MapStyle ?? string.Empty,
            map.MapType ?? string.Empty,
            map.MapUid,
            map.MapName,
            true);

        return new MultipartFormDataContent
        {
            { JsonContent.Create(mapInfo, NadeoAPIMapInfoJsonContext.Default.MapInfoSubmit), "nadeoservices-core-parameters" },
            { new StreamContent(bufferedStream), "data", fileName }
        };
    }
}

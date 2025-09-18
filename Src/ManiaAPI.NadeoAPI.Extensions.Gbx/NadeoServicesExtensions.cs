using GBX.NET;
using GBX.NET.Engines.Game;
using System.Net.Http.Json;
using System.Text;
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

        await using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        return await UploadMapAsync(services, content, cancellationToken);
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
    /// <param name="mapGbx"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, Gbx<CGameCtnChallenge> mapGbx, GbxWriteSettings settings = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapGbx);

        if (string.IsNullOrWhiteSpace(mapGbx.FilePath))
        {
            throw new ArgumentException("The file path in Gbx is invalid.", nameof(mapGbx));
        }

        await using var stream = new MemoryStream();
        mapGbx.Save(stream, settings);
        stream.Position = 0;

        using var content = CreateContent(stream, Path.GetFileName(mapGbx.FilePath), mapGbx.Node);

        return await UploadMapAsync(services, content, cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="map"></param>
    /// <param name="fileName"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, CGameCtnChallenge map, string fileName, GbxWriteSettings settings = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(fileName);

        await using var stream = new MemoryStream();
        map.Save(stream, settings);
        stream.Position = 0;

        using var content = CreateContent(stream, fileName, map);

        return await UploadMapAsync(services, content, cancellationToken);
    }

    private static async Task<MapInfo> UploadMapAsync(INadeoServices services, MultipartFormDataContent content, CancellationToken cancellationToken)
    {
        using var response = await services.SendAsync(HttpMethod.Post, "maps/", content, cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
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

        await using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        return await UpdateMapAsync(services, mapId, content, cancellationToken);
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
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        return await UpdateMapAsync(services, mapId, stream, Path.GetFileName(filePath), cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="mapGbx"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, Gbx<CGameCtnChallenge> mapGbx, GbxWriteSettings settings = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapGbx);

        if (string.IsNullOrWhiteSpace(mapGbx.FilePath))
        {
            throw new ArgumentException("The file path in Gbx is invalid.", nameof(mapGbx));
        }

        await using var stream = new MemoryStream();
        mapGbx.Save(stream, settings);
        stream.Position = 0;

        using var content = CreateContent(stream, Path.GetFileName(mapGbx.FilePath), mapGbx.Node);

        return await UpdateMapAsync(services, mapId, content, cancellationToken);
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="map"></param>
    /// <param name="fileName"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, CGameCtnChallenge map, string fileName, GbxWriteSettings settings = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(fileName);

        await using var stream = new MemoryStream();
        map.Save(stream, settings);
        stream.Position = 0;

        using var content = CreateContent(stream, fileName, map);

        return await UpdateMapAsync(services, mapId, content, cancellationToken);
    }

    private static async Task<MapInfo> UpdateMapAsync(INadeoServices services, Guid mapId, MultipartFormDataContent content, CancellationToken cancellationToken)
    {
        using var response = await services.SendAsync(HttpMethod.Post, $"maps/{mapId}", content, cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }


    private static MultipartFormDataContent CreateContent(BufferedStream bufferedStream, string fileName)
    {
        var map = GBX.NET.Gbx.ParseHeaderNode<CGameCtnChallenge>(bufferedStream);

        bufferedStream.Position = 0;

        return CreateContent(bufferedStream, fileName, map);
    }

    private static MultipartFormDataContent CreateContent(Stream stream, string fileName, CGameCtnChallenge map)
    {
        return new MultipartFormDataContent
        {
            { new StringContent(map.AuthorTime?.TotalMilliseconds.ToString() ?? "-1"), "authorScore" },
            { new StringContent(map.GoldTime?.TotalMilliseconds.ToString() ?? "-1"), "goldScore" },
            { new StringContent(map.SilverTime?.TotalMilliseconds.ToString() ?? "-1"), "silverScore" },
            { new StringContent(map.BronzeTime?.TotalMilliseconds.ToString() ?? "-1"), "bronzeScore" },
            { new StringContent(AccountUtils.ToAccountId(map.AuthorLogin).ToString()), "author" },
            { new StringContent(map.Collection.HasValue && map.Collection.Value.Number != 26 ? map.Collection : "Stadium"), "collectionName" },
            { new StringContent(map.MapStyle ?? string.Empty), "mapStyle" },
            { new StringContent(map.MapType ?? string.Empty), "mapType" },
            { new StringContent(map.MapUid), "mapUid" },
            { new StringContent(map.MapName), "name" },
            { new StringContent(@"{""isPlayable"":true}", Encoding.UTF8, "application/json"), "nadeoservices-core-parameters" },
            { new StreamContent(stream), "data", fileName }
        };
    }
}

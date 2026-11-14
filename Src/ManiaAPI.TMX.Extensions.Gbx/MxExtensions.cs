using GBX.NET;
using GBX.NET.Engines.Game;

namespace ManiaAPI.TMX.Extensions.Gbx;

public static class MxExtensions
{
    public static async Task<Gbx<CGameCtnChallenge>> GetMapGbxHeaderAsync(this MX mx, long mapId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await mx.GetMapGbxResponseAsync(mapId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return GBX.NET.Gbx.ParseHeader<CGameCtnChallenge>(stream, settings);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetMapGbxHeaderAsync(this MX mx, MapItem map, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetMapGbxHeaderAsync(map.MapId, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetMapGbxAsync(this MX mx, long mapId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await mx.GetMapGbxResponseAsync(mapId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await GBX.NET.Gbx.ParseAsync<CGameCtnChallenge>(stream, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetMapGbxAsync(this MX mx, MapItem map, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetMapGbxAsync(map.MapId, settings, cancellationToken);
    }

    public static async Task<CGameCtnChallenge> GetMapGbxHeaderNodeAsync(this MX mx, long mapId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await mx.GetMapGbxHeaderAsync(mapId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnChallenge> GetMapGbxHeaderNodeAsync(this MX mx, MapItem map, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetMapGbxHeaderNodeAsync(map.MapId, settings, cancellationToken);
    }

    /// <summary>
    /// With just a direct node, some Gbx properties are lost and need to be reintroduced when serializing the object back to Gbx.
    /// </summary>
    public static async Task<CGameCtnChallenge> GetMapGbxNodeAsync(this MX mx, long mapId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await mx.GetMapGbxAsync(mapId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnChallenge> GetMapGbxNodeAsync(this MX mx, MapItem map, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetMapGbxNodeAsync(map.MapId, settings, cancellationToken);
    }



    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxHeaderAsync(this MX mx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await mx.GetReplayGbxResponseAsync(replayId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return GBX.NET.Gbx.ParseHeader<CGameCtnReplayRecord>(stream, settings);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxHeaderAsync(this MX mx, ReplayItemMX replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetReplayGbxHeaderAsync(replay.ReplayId, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxAsync(this MX mx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await mx.GetReplayGbxResponseAsync(replayId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await GBX.NET.Gbx.ParseAsync<CGameCtnReplayRecord>(stream, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxAsync(this MX mx, ReplayItemMX replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetReplayGbxAsync(replay.ReplayId, settings, cancellationToken);
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxHeaderNodeAsync(this MX mx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await mx.GetReplayGbxHeaderAsync(replayId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxHeaderNodeAsync(this MX mx, ReplayItemMX replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetReplayGbxHeaderNodeAsync(replay.ReplayId, settings, cancellationToken);
    }

    /// <summary>
    /// With just a direct node, some Gbx properties are lost and need to be reintroduced when serializing the object back to Gbx.
    /// </summary>
    public static async Task<CGameCtnReplayRecord> GetReplayGbxNodeAsync(this MX mx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await mx.GetReplayGbxAsync(replayId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxNodeAsync(this MX mx, ReplayItemMX replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await mx.GetReplayGbxNodeAsync(replay.ReplayId, settings, cancellationToken);
    }
}
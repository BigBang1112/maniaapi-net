using GBX.NET;
using GBX.NET.Engines.Game;

namespace ManiaAPI.TMX.Extensions.Gbx;

public static class TmxExtensions
{
    public static async Task<Gbx<CGameCtnChallenge>> GetTrackGbxHeaderAsync(this TMX tmx, long trackId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await tmx.GetTrackGbxResponseAsync(trackId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return GBX.NET.Gbx.ParseHeader<CGameCtnChallenge>(stream, settings);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetTrackGbxHeaderAsync(this TMX tmx, TrackItem track, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetTrackGbxHeaderAsync(track.TrackId, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetTrackGbxAsync(this TMX tmx, long trackId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await tmx.GetTrackGbxResponseAsync(trackId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await GBX.NET.Gbx.ParseAsync<CGameCtnChallenge>(stream, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnChallenge>> GetTrackGbxAsync(this TMX tmx, TrackItem track, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetTrackGbxAsync(track.TrackId, settings, cancellationToken);
    }

    public static async Task<CGameCtnChallenge> GetTrackGbxHeaderNodeAsync(this TMX tmx, long trackId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await tmx.GetTrackGbxHeaderAsync(trackId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnChallenge> GetTrackGbxHeaderNodeAsync(this TMX tmx, TrackItem track, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetTrackGbxHeaderNodeAsync(track.TrackId, settings, cancellationToken);
    }

    /// <summary>
    /// With just a direct node, some Gbx properties are lost and need to be reintroduced when serializing the object back to Gbx.
    /// </summary>
    /// <param name="tmx"></param>
    /// <param name="trackId"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<CGameCtnChallenge> GetTrackGbxNodeAsync(this TMX tmx, long trackId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await tmx.GetTrackGbxAsync(trackId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnChallenge> GetTrackGbxNodeAsync(this TMX tmx, TrackItem track, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetTrackGbxNodeAsync(track.TrackId, settings, cancellationToken);
    }



    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxHeaderAsync(this TMX tmx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await tmx.GetReplayGbxResponseAsync(replayId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return GBX.NET.Gbx.ParseHeader<CGameCtnReplayRecord>(stream, settings);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxHeaderAsync(this TMX tmx, ReplayItem replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetReplayGbxHeaderAsync(replay.ReplayId, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxAsync(this TMX tmx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        using var response = await tmx.GetReplayGbxResponseAsync(replayId, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await GBX.NET.Gbx.ParseAsync<CGameCtnReplayRecord>(stream, settings, cancellationToken);
    }

    public static async Task<Gbx<CGameCtnReplayRecord>> GetReplayGbxAsync(this TMX tmx, ReplayItem replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetReplayGbxAsync(replay.ReplayId, settings, cancellationToken);
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxHeaderNodeAsync(this TMX tmx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await tmx.GetReplayGbxHeaderAsync(replayId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxHeaderNodeAsync(this TMX tmx, ReplayItem replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetReplayGbxHeaderNodeAsync(replay.ReplayId, settings, cancellationToken);
    }

    /// <summary>
    /// With just a direct node, some Gbx properties are lost and need to be reintroduced when serializing the object back to Gbx.
    /// </summary>
    /// <param name="tmx"></param>
    /// <param name="replayId"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<CGameCtnReplayRecord> GetReplayGbxNodeAsync(this TMX tmx, long replayId, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return (await tmx.GetReplayGbxAsync(replayId, settings, cancellationToken)).Node;
    }

    public static async Task<CGameCtnReplayRecord> GetReplayGbxNodeAsync(this TMX tmx, ReplayItem replay, GbxReadSettings settings = default, CancellationToken cancellationToken = default)
    {
        return await tmx.GetReplayGbxNodeAsync(replay.ReplayId, settings, cancellationToken);
    }
}

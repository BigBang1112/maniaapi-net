namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// The ManiaPlanet Channel Record API (records.prod.live.maniaplanet.com), which holds the leaderboards of channel maps.
/// Only its public read routes are wrapped; the others are for game servers.
/// </summary>
public interface IManiaPlanetChannelRecordAPI
{
    HttpClient Client { get; }

    /// <summary>
    /// Gets the record and rank of a player on a map.
    /// </summary>
    /// <param name="mapUid">The unique ID (MapUid) of the map.</param>
    /// <param name="login">Player login.</param>
    /// <param name="type">Leaderboard type, usually <c>channel</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The player's record with the number of records on the map, or null when the map has no leaderboard of this type.</returns>
    Task<ChannelPlayerRecord?> GetPlayerRecordAsync(string mapUid, string login, string type = ManiaPlanetChannelRecordAPI.ChannelType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the top 10 records of a map, the number of records, and the ranks of the given players.
    /// </summary>
    /// <param name="mapUid">The unique ID (MapUid) of the map.</param>
    /// <param name="logins">Logins of players to get the ranks of.</param>
    /// <param name="type">Leaderboard type, usually <c>channel</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The leaderboard, or null when the map has no leaderboard of this type.</returns>
    Task<ChannelLeaderboard?> GetLeaderboardAsync(string mapUid, IEnumerable<string>? logins = null, string type = ManiaPlanetChannelRecordAPI.ChannelType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of the records of a map, ordered by rank.
    /// </summary>
    /// <param name="mapUid">The unique ID (MapUid) of the map.</param>
    /// <param name="offset">Number of records to skip.</param>
    /// <param name="length">Number of records to get, at most 101.</param>
    /// <param name="type">Leaderboard type, usually <c>channel</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The records, empty when the map has no leaderboard of this type or the page is past the last record.</returns>
    Task<IReadOnlyList<ChannelRecord>> GetRecordsAsync(string mapUid, int offset = 0, int length = 100, string type = ManiaPlanetChannelRecordAPI.ChannelType, CancellationToken cancellationToken = default);
}

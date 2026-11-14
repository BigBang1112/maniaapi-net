using System.Collections.Immutable;

namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// The top of a channel leaderboard of a map, with the ranks of requested players.
/// </summary>
/// <param name="Top">The top 10 records, ordered by rank.</param>
/// <param name="RecordCount">Number of records on the map.</param>
/// <param name="PlayerRanks">Ranks of the requested players by login. Players without a record are left out.</param>
public sealed record ChannelLeaderboard(ImmutableList<ChannelRecord> Top, int RecordCount, ImmutableDictionary<string, int> PlayerRanks);

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

/// <summary>A cached leaderboard and its collection and expiry times.</summary>
/// <remarks>Data contains rankings for solo/multi snapshots, or a track leaderboard object for track snapshots.</remarks>
public sealed record LeaderboardSnapshot(int Id, string Type, string Zone, string Environment,
                                         [property: JsonPropertyName("offset_value")] int Offset,
                                         [property: JsonPropertyName("snapshot_data")] JsonElement Data,
                                         [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
                                         [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt)
{
    /// <summary>Reads the player rankings, including the records in a track snapshot.</summary>
    public ImmutableList<PlayerRanking> GetRankings()
    {
        var rankings = Type == "track" ? Data.GetProperty("records") : Data;
        return rankings.Deserialize(UnitedLadderJsonContext.Default.ImmutableListPlayerRanking)
            ?? throw new JsonException("The snapshot rankings are null.");
    }

    /// <summary>Reads a track snapshot, or returns null for solo/multi snapshots.</summary>
    public TrackLeaderboard? GetTrackLeaderboard()
    {
        return Type == "track" ? Data.Deserialize(UnitedLadderJsonContext.Default.TrackLeaderboard)
            ?? throw new JsonException("The snapshot track leaderboard is null.") : null;
    }
}

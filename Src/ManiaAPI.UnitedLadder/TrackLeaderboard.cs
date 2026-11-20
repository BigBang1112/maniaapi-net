using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record TrackLeaderboard(string Uid, string Name, string Author, string Envi, string GameMode,
                                      int RecordCount, ImmutableList<TrackRecord> Records);

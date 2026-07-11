using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubTopLeaderboard(string GroupUid, string MapUid, int ClubId, int Length, ImmutableList<Record> Top);

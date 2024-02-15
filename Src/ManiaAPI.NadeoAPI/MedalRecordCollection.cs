using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MedalRecordCollection(string GroupUid, string MapUid, ImmutableArray<MedalRecord> Medals);

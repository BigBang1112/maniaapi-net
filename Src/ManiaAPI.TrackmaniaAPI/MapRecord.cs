namespace ManiaAPI.TrackmaniaAPI;

public sealed record MapRecord(Guid AccountId,
                               string GameMode,
                               string GameModeCustomData,
                               Guid MapId,
                               string MapRecordId,
                               int Medal,
                               bool Removed,
                               int RespawnCount,
                               string ScopeId,
                               string ScopeType,
                               int Score,
                               int Time,
                               DateTimeOffset Timestamp,
                               string Url);
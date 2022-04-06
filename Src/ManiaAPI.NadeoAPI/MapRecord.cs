using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public record MapRecord(Guid AccountId,
                        string FileName,
                        string GameMode,
                        string GameModeCustomData,
                        Guid MapId,
                        Guid MapRecordId,
                        [property: JsonPropertyName("medal")] int Medals,
                        RecordScore RecordScore,
                        bool Removed,
                        Guid? ScopeId,
                        string ScopeName,
                        DateTimeOffset Timestamp,
                        string Url)
{
    public override string ToString()
    {
        return $"{RecordScore.Time} by {AccountId} on {MapId}";
    }
}
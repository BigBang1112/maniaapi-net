using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public record Record(Guid AccountId, Guid ZoneId, string ZoneName, int Position, TimeInt32 Score)
{
    public override string ToString()
    {
        return $"{Position}) {Score} by {AccountId}";
    }
}
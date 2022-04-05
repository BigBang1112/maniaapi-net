using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public record Record(int Position,
                     TimeInt32 Time,
                     string FileName,
                     DateTimeOffset Timestamp,
                     string Url,
                     Player Player)
{
    public override string ToString()
    {
        return $"{Time} by {Player}";
    }
}
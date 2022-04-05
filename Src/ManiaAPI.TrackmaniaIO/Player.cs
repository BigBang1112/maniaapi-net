namespace ManiaAPI.TrackmaniaIO;

public record Player(string Name, string? Tag, Guid Id, Zone Zone, Meta? Meta)
{
    public override string ToString()
    {
        return Name;
    }
}
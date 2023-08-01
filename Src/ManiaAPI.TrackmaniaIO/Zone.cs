namespace ManiaAPI.TrackmaniaIO;

public sealed record Zone(string Name, string Flag, Zone Parent)
{
    public override string ToString()
    {
        return Name;
    }
}
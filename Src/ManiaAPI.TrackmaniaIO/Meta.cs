namespace ManiaAPI.TrackmaniaIO;

public record Meta
{
    public string Vanity { get; init; } = "";
    public string Comment { get; init; } = "";
    public bool Nadeo { get; init; }
    public string Twitch { get; init; } = "";
    public string YouTube { get; init; } = "";
    public string Twitter { get; init; } = "";
}

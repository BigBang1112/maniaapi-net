namespace ManiaAPI.NadeoAPI;

public sealed record ClubNewsEdition
{
    public string? Name { get; init; }
    public string? Headline { get; init; }
    public string? Body { get; init; }
}

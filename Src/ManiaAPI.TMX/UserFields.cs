namespace ManiaAPI.TMX;

public record UserFields
{
    public bool UserId { get; init; }
    public bool Name { get; init; }

    public static readonly UserFields All = new()
    {
        UserId = true,
        Name = true
    };
}
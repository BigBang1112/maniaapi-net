using ManiaAPI.TMX.Attributes;
using System.Collections.Immutable;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record MappackItem : IItem
{
    public long MappackId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Type { get; set; }
    public bool IsRequest { get; set; }
    public int SubmitLimit { get; set; }
    public double MappackValue { get; set; }
    public int MapCount { get; set; }
    public bool IsPublic { get; set; }
    public bool MaplistReleased { get; set; }
    public string? VideoUrl { get; set; }
    public User Owner { get; set; } = default!;
    public ImmutableList<Tag>? Tags { get; set; }
    public ImmutableList<MappackManager>? Managers { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RequestEndAt { get; set; }
    public DateTimeOffset? MaplistAt { get; set; }
    public DateTimeOffset? ActivityAt { get; set; }
    public DateTimeOffset? LBEndAt { get; set; }
    // public MappackImage? Image { get; set; } // in the docs, but doesn't actually exist'
}
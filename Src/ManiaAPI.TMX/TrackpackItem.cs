using ManiaAPI.TMX.Attributes;
using System.Collections.Immutable;

namespace ManiaAPI.TMX;

[Fields]
public sealed record TrackpackItem : IItem
{
    public long PackId { get; set; }
    public string PackName { get; set; } = default!;
    public int Tracks { get; set; }
    public int PackValue { get; set; }
    public bool AllowsTrackSubmissions { get; set; }
    public bool IsLegacy { get; set; }
    public int Downloads { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public User Creator { get; set; } = default!;
    public ImmutableArray<Author> Managers { get; set; } = default!;
}

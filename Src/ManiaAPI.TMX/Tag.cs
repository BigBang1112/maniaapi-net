using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record Tag
{
    public int TagId { get; set; }
    public string Name { get; set; } = default!;
    public string Color { get; set; } = default!;
}
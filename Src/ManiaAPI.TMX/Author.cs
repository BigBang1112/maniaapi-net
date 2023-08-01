using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed record Author(User User, string Role);

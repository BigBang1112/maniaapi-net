using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record Author(User User, string Role);

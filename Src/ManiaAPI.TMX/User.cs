using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record User(int UserId, string Name);
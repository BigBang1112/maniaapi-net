using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public partial record struct MapFeature(string? Comment, bool Pinned);
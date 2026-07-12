using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public partial record struct MappackInfo(long MappackId, int MapStatus, int MapPosition);
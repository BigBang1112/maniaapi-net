using ManiaAPI.TMX.Attributes;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public record UserReplay(int ReplayId, TimeInt32 ReplayTime, int ReplayScore);
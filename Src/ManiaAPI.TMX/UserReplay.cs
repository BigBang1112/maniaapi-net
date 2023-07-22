using ManiaAPI.TMX.Attributes;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public sealed record UserReplay(int ReplayId, TimeInt32 ReplayTime, int ReplayScore);
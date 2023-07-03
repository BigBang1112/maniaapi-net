using ManiaAPI.TMX.Attributes;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public record WRReplay(User User, TimeInt32 ReplayTime, int ReplayScore, int ReplayId);
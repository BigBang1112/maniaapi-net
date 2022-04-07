using TmEssentials;

namespace ManiaAPI.TMX;

public record Replay(User User, int ReplayId, TimeInt32 ReplayTime, int ReplayScore);
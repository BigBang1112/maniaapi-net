using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed record PlayerAchievements(int Count, DateTimeOffset Aa, ImmutableArray<PlayerAchievement> Maps);

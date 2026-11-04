using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMUF;

public sealed record PlayerAchievements(int Count, DateTimeOffset Aa, ImmutableList<PlayerAchievement> Maps);

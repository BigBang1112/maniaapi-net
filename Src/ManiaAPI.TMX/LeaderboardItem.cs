﻿using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed record LeaderboardItem(User User, int ReplayScore, int ReplayWRs, int Top10s, int Replays, int Position, int Delta) : IItem;

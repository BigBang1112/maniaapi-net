﻿namespace ManiaAPI.Xml.TMUF;

public sealed class CampaignScoresLeaderboard(IReadOnlyDictionary<string, Leaderboard> challengeScores)
{
    public IReadOnlyDictionary<string, Leaderboard> ChallengeScores { get; } = challengeScores;
}
namespace ManiaAPI.XmlRpc.TMUF;

public class CampaignScoresLeaderboard
{
    public IReadOnlyDictionary<string, Leaderboard> ChallengeScores { get; }

    public CampaignScoresLeaderboard(Dictionary<string, Leaderboard> challengeScores)
    {
        ChallengeScores = challengeScores;
    }
}
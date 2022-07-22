using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMUF;

public class Leaderboard
{
    public ImmutableArray<HighScore> HighScores { get; }
    public ImmutableArray<RecordUnit> Skillpoints { get; }

    public Leaderboard(HighScore[] highScores, RecordUnit[] skillpoints)
    {
        HighScores = ImmutableArray.Create(highScores);
        Skillpoints = ImmutableArray.Create(skillpoints);
    }
}

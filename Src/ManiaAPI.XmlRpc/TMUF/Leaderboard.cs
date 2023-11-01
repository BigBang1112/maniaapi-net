using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMUF;

public sealed class Leaderboard(HighScore[] highScores, RecordUnit[] skillpoints)
{
    public ImmutableArray<HighScore> HighScores { get; } = ImmutableArray.Create(highScores);
    public ImmutableArray<RecordUnit> Skillpoints { get; } = ImmutableArray.Create(skillpoints);
}

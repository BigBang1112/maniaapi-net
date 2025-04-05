using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed class Leaderboard(HighScore[] highScores, RecordUnit<uint>[] skillpoints)
{
    public ImmutableArray<HighScore> HighScores { get; } = ImmutableArray.Create(highScores);
    public ImmutableArray<RecordUnit<uint>> Skillpoints { get; } = ImmutableArray.Create(skillpoints);
}

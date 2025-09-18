using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMUF;

public sealed class Leaderboard(HighScore[] highScores, RecordUnit<uint>[] skillpoints)
{
    public ImmutableList<HighScore> HighScores { get; } = ImmutableList.Create(highScores);
    public ImmutableList<RecordUnit<uint>> Skillpoints { get; } = ImmutableList.Create(skillpoints);
}

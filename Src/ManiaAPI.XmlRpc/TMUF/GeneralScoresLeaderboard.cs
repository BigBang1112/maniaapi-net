namespace ManiaAPI.XmlRpc.TMUF;

public class GeneralScoresLeaderboard
{
    public GeneralScoresHighScore[] HighScores { get; }
    public GeneralScoresRecordUnit[] Skillpoints { get; }

    public GeneralScoresLeaderboard(GeneralScoresHighScore[] highScores, GeneralScoresRecordUnit[] skillpoints)
    {
        HighScores = highScores;
        Skillpoints = skillpoints;
    }
}

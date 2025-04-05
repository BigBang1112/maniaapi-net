using TmEssentials;

namespace ManiaAPI.Xml.TMUF;

public sealed record HighScore(int Rank, int Score, string Login, string Nickname)
{
    public TimeInt32 Time => new(Score);
}

using TmEssentials;

namespace ManiaAPI.XmlRpc.TMUF;

public record HighScore(int Rank, int Score, string Login, string Nickname)
{
    public TimeInt32 Time => new(Score);
}

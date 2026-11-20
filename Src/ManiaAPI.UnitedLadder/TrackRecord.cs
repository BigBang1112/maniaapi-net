namespace ManiaAPI.UnitedLadder;

public sealed record TrackRecord(int Points, int Id, string Login, string Nickname, string Path,
                                 int GlobalRank, long Skillpoints, string? Ghost);

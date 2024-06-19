namespace ManiaAPI.NadeoAPI;

public sealed record ClubPlayerInfo(bool HasClubVip,
                                    bool HasPlayerVip,
                                    bool HasFollower,
                                    int TagClubId,
                                    string Tag,
                                    int PinnedClub);
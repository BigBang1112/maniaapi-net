using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMemberCollection(ImmutableList<ClubMember> ClubMemberList,
                                          int MaxPage,
                                          int ItemCount);

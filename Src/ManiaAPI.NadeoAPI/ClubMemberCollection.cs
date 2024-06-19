using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMemberCollection(ImmutableArray<ClubMember> ClubMemberList,
                                          int MaxPage,
                                          int ItemCount);

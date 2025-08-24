using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record Player(int Id,
                            string Login,
                            string Nickname,
                            bool United,
                            string Path,
                            int IdZone,
                            bool Legacy,
                            ImmutableList<Tag>? Tags);

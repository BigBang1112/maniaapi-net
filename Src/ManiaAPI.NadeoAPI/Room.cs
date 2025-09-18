using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record Room(int? Id,
                          string Name,
                          string? Region,
                          string ServerAccountId,
                          int MaxPlayers,
                          int PlayerCount,
                          ImmutableList<string> Maps,
                          string Script,
                          bool Scalable
                          //ImmutableDictionary<string, ScriptSetting> ScriptSettings bugged atm
    );

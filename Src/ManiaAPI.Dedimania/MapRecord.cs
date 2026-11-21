using TmEssentials;

namespace ManiaAPI.Dedimania;

/// <param name="Time">Time of the record.</param>
/// <param name="Mode">Game mode the record was driven in.</param>
/// <param name="Timestamp">When the record was driven, or null when Dedimania doesn't know.</param>
/// <param name="Login">Login of the player. On TMF, Dedimania prefixes logins, as in <c>TMU:player</c>.</param>
/// <param name="ServerLogin">Login of the server the record was driven on, or null when Dedimania doesn't know. Prefixed on TMF like <see cref="Login"/>.</param>
/// <param name="Nickname">Formatted nickname of the player. Commas are replaced with dots.</param>
/// <param name="ServerName">Formatted name of the server the record was driven on. Commas are replaced with dots.</param>
public sealed record MapRecord(TimeInt32 Time,
                               GameMode Mode,
                               DateTimeOffset? Timestamp,
                               string Login,
                               string? ServerLogin,
                               string Nickname,
                               string ServerName);

using System.Collections.Immutable;

namespace ManiaAPI.Dedimania;

/// <param name="Uid">UID of the map.</param>
/// <param name="Environment">Environment of the map, like <c>Stadium</c> or <c>Canyon</c>.</param>
/// <param name="AuthorLogin">Login of the map author.</param>
/// <param name="TimesPlayed">How many times the map was played on servers sending records to Dedimania.</param>
/// <param name="Name">Formatted name of the map. Commas are replaced with dots.</param>
/// <param name="Records">Best records of the map, at most 100.</param>
public sealed record Map(string Uid,
                         string Environment,
                         string AuthorLogin,
                         int TimesPlayed,
                         string Name,
                         ImmutableList<MapRecord> Records);

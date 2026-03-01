namespace ManiaAPI.Xml;

/// <summary>
/// CheckLogin result
/// </summary>
/// <param name="Exists">Login exists.</param>
/// <param name="Paid">Account has bought United. Null for other games.</param>
/// <param name="Migrated">Account was migrated from ManiaPlanet 3 to ManiaPlanet 4. Always true for existing TMTurbo accounts though.</param>
public sealed record CheckLoginResult(bool Exists, bool? Paid, bool Migrated);

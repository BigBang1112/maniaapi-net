namespace ManiaAPI.MasterServer;

public sealed partial record PlayerInfos(string Login, string Nickname, string Zone, DateTimeOffset? CreatedAt, int D, DateTimeOffset? LastZoneUpdate, int? K);

namespace ManiaAPI.NadeoAPI;

public sealed record PlayerServerAccount(Guid AccountId, string Login, bool AlreadyUsed, int? ClubRoomId, string? ClubRoomName);

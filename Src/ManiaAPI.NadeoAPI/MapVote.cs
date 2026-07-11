namespace ManiaAPI.NadeoAPI;

public sealed record MapVote(Guid AccountId, string MapUid, int Vote, DateTimeOffset VoteDate);

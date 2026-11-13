namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A range of ranks to request, both inclusive and starting at 1.
/// </summary>
internal sealed record ChannelRecordsRequest(string MapUid, int RangeStart, int RangeEnd, string Type);

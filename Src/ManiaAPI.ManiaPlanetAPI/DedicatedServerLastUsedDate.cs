namespace ManiaAPI.ManiaPlanetAPI;

public sealed record DedicatedServerLastUsedDate(
    DedicatedServerTimeZone Timezone,
    int Offset,
    long Timestamp);

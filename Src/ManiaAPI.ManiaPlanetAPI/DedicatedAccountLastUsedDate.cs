namespace ManiaAPI.ManiaPlanetAPI;

public sealed record DedicatedAccountLastUsedDate(
    DedicatedAccountTimeZone Timezone,
    int Offset,
    long Timestamp);

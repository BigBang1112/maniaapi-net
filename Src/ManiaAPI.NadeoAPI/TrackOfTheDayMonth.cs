namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayMonth(int Year, int Month, int LastDay, TrackOfTheDay[] Days, Media Media);
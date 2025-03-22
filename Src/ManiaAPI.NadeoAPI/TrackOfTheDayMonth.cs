using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayMonth(int Year, int Month, int LastDay, ImmutableArray<TrackOfTheDay> Days, Media Media);
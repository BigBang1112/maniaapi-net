using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayMonth(int Year, int Month, int LastDay, ImmutableList<TrackOfTheDay> Days, Media Media);
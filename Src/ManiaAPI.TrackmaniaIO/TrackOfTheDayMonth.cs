using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record TrackOfTheDayMonth(int Year,
                                        int Month,
                                        [property: JsonPropertyName("lastday")] int LastDay,
                                        ImmutableList<TrackOfTheDay> Days,
                                        [property: JsonPropertyName("monthoffset")] int MonthOffset,
                                        [property: JsonPropertyName("monthcount")] int MonthCount);
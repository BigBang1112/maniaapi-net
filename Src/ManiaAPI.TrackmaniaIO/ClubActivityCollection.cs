using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubActivityCollection(ImmutableList<ClubActivity> Activities, int Page, [property: JsonPropertyName("page_max")] int PageMax);
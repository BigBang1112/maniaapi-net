using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CompetitionCollection(ImmutableArray<CompetitionItem> Competitions, int Page, [property: JsonPropertyName("page_max")] int PageMax);
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubMemberCollection(ImmutableArray<ClubMember> Members, int Page, [property: JsonPropertyName("page_max")] int PageMax);
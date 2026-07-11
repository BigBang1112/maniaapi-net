using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMapReviewCreation
{
    public required string Name { get; init; }
    public int? TimeLimit { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? Scalable { get; init; }

    public int? MaxPlayer { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? AllowVoteSkipMap { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? SubmissionLimitation { get; init; }

    public int? FolderId { get; init; }
}

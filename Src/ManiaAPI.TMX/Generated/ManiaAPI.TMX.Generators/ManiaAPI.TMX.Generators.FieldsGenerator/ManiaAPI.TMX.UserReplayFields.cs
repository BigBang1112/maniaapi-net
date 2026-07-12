using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct UserReplayFields
{
    public bool ReplayId { get; init; }
    public bool ReplayTime { get; init; }
    public bool ReplayScore { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly UserReplayFields All = new()
    {
        ReplayId = true,
        ReplayTime = true,
        ReplayScore = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayId));
            first = false;
        }

        if (ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayTime));
            first = false;
        }

        if (ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayScore));
            first = false;
        }

        if (AdditionalFields is not null)
        {
            foreach (var additionalField in AdditionalFields)
            {
                if (!first) sb.Append("%2C");
                sb.Append(additionalField);
                first = false;
            }
        }

        return !first;
    }
}

public partial record UserReplay
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}

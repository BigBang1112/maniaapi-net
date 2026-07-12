using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct ReplayItemFields
{
    public bool ReplayId { get; init; }
    public bool ReplayTime { get; init; }
    public bool ReplayScore { get; init; }
    public bool ReplayRespawns { get; init; }
    public bool ReplayAt { get; init; }
    public bool TrackAt { get; init; }
    public bool Position { get; init; }
    public bool IsBest { get; init; }
    public bool Score { get; init; }
    public bool IsLeaderboard { get; init; }
    public bool Validated { get; init; }
    public global::ManiaAPI.TMX.UserFields User { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly ReplayItemFields All = new()
    {
        ReplayId = true,
        ReplayTime = true,
        ReplayScore = true,
        ReplayRespawns = true,
        ReplayAt = true,
        TrackAt = true,
        Position = true,
        IsBest = true,
        Score = true,
        IsLeaderboard = true,
        Validated = true,
        User = global::ManiaAPI.TMX.UserFields.All,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.ReplayId));
            first = false;
        }

        if (ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.ReplayTime));
            first = false;
        }

        if (ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.ReplayScore));
            first = false;
        }

        if (ReplayRespawns)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.ReplayRespawns));
            first = false;
        }

        if (ReplayAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.ReplayAt));
            first = false;
        }

        if (TrackAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.TrackAt));
            first = false;
        }

        if (Position)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.Position));
            first = false;
        }

        if (IsBest)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.IsBest));
            first = false;
        }

        if (Score)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.Score));
            first = false;
        }

        if (IsLeaderboard)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.IsLeaderboard));
            first = false;
        }

        if (Validated)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.Validated));
            first = false;
        }

        if (User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItem.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
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

public partial record ReplayItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}

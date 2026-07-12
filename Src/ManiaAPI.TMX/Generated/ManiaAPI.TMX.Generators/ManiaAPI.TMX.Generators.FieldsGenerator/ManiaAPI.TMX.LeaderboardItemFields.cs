using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct LeaderboardItemFields
{
    public global::ManiaAPI.TMX.UserFields User { get; init; }
    public bool ReplayScore { get; init; }
    public bool ReplayWRs { get; init; }
    public bool Top10s { get; init; }
    public bool Replays { get; init; }
    public bool Position { get; init; }
    public bool Delta { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly LeaderboardItemFields All = new()
    {
        User = global::ManiaAPI.TMX.UserFields.All,
        ReplayScore = true,
        ReplayWRs = true,
        Top10s = true,
        Replays = true,
        Position = true,
        Delta = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.ReplayScore));
            first = false;
        }

        if (ReplayWRs)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.ReplayWRs));
            first = false;
        }

        if (Top10s)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.Top10s));
            first = false;
        }

        if (Replays)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.Replays));
            first = false;
        }

        if (Position)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.Position));
            first = false;
        }

        if (Delta)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.LeaderboardItem.Delta));
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

public partial record LeaderboardItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}

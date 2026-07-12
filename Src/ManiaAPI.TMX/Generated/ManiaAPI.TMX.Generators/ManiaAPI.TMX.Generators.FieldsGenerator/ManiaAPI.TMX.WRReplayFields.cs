using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct WRReplayFields
{
    public global::ManiaAPI.TMX.UserFields User { get; init; }
    public bool ReplayTime { get; init; }
    public bool ReplayScore { get; init; }
    public bool ReplayId { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly WRReplayFields All = new()
    {
        User = global::ManiaAPI.TMX.UserFields.All,
        ReplayTime = true,
        ReplayScore = true,
        ReplayId = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayTime));
            first = false;
        }

        if (ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayScore));
            first = false;
        }

        if (ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayId));
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

public partial record WRReplay
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
